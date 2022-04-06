using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using MowingMachine.Models;

namespace MowingMachine.Common
{
    public static class MapUtilities
    {
        private static readonly Dictionary<string, Bitmap> _CachedBitmaps = new();
        private static readonly Dictionary<string, BitmapImage> _CachedBitmapImages = new();

        private static string PathToImage(FieldType fieldType) => $"./assets/{fieldType}.png";

        private static bool GetCachedBitmap(string key, out Bitmap bitmap)
        {
            if (_CachedBitmaps.TryGetValue(key, out bitmap))
            {
                bitmap = (Bitmap) bitmap.Clone();
                return true;
            }
        
            return false;
        }

        private static void SetCachedBitmap(string key, Bitmap bitmap)
            => _CachedBitmaps[key] = (Bitmap) bitmap.Clone();

        private static bool GetCachedBitmapImage(string key, out BitmapImage bitmapImage)
        {
            if (_CachedBitmapImages.TryGetValue(key, out bitmapImage))
            {
                bitmapImage = bitmapImage.Clone();
                return true;
            }
            
            return false;
        }

        private static void SetCachedBitmapImage(string key, BitmapImage bitmap)
            => _CachedBitmapImages[key] = bitmap.Clone();

        public static BitmapImage GetFieldBitmapWithTransition(
            FieldType fieldType,
            FieldType fieldTop,
            FieldType fieldRight,
            FieldType fieldBottom,
            FieldType fieldLeft)
        {
            if (fieldType is FieldType.MowingMachine or FieldType.ChargingStation or FieldType.Water)
            {
                return fieldType switch
                {
                    FieldType.ChargingStation or FieldType.MowingMachine or FieldType.Water
                        => FieldTypeToDefaultBitmapImage(fieldType),
                    _ => throw new ArgumentOutOfRangeException(nameof(fieldType), fieldType, null),
                };
            }
            
            var key = GetCacheKey(fieldType, fieldTop, fieldRight, fieldBottom, fieldLeft);

            if (GetCachedBitmapImage(key, out var image))
                return image;

            if (!GetCachedBitmap(GetCacheKey(fieldType), out var defaultImage))
            {
                defaultImage = GetImageAsBitmap($"./assets/{fieldType}.png");
                SetCachedBitmap(GetCacheKey(fieldType), defaultImage);
            }

            var images = new List<Bitmap>
            {
                GetFieldSingleSideTransition
                    (defaultImage.Width, defaultImage.Height, fieldType, MoveDirection.Top, fieldTop),
                GetFieldSingleSideTransition
                    (defaultImage.Width, defaultImage.Height, fieldType, MoveDirection.Right, fieldRight),
                GetFieldSingleSideTransition
                    (defaultImage.Width, defaultImage.Height, fieldType, MoveDirection.Bottom, fieldBottom),
                GetFieldSingleSideTransition
                    (defaultImage.Width, defaultImage.Height, fieldType, MoveDirection.Left, fieldLeft),
            };
            
            images.Insert(0, defaultImage);
            
            image = images.Where(i => i is not null)
                .MergeImages()
                .ToBitmapImage();
            
            SetCachedBitmapImage(key, image);
            return image;
        }
        
        private static Bitmap GetFieldSingleSideTransition(
            double fieldImageWidth,
            double fieldImageHeight,
            FieldType fieldType,
            MoveDirection direction,
            FieldType neighborFieldType)
        {
            var successful =
                GetCachedBitmap(GetCacheKey(direction, fieldType, neighborFieldType), out var cachedBitmap);
            
            if (successful)
                return cachedBitmap;
            
            if ((int) neighborFieldType == -1 
                || neighborFieldType is FieldType.MowingMachine or FieldType.Water or FieldType.ChargingStation
                || fieldType == neighborFieldType)
                return null;

            successful = GetCachedBitmap(GetCacheKey(fieldType, neighborFieldType), out var transitionImage);

            if (!successful)
            {
                transitionImage = CreateImageTransition(fieldType, neighborFieldType);
                SetCachedBitmap(GetCacheKey(fieldType, neighborFieldType), transitionImage);
            }

            var height = transitionImage.Height;
            var width = transitionImage.Width;
            
            if (direction is MoveDirection.Bottom or MoveDirection.Top)
                transitionImage.RotateFlip(RotateFlipType.Rotate90FlipNone);

            var point = direction switch
            {
                MoveDirection.Left or MoveDirection.Top => new Point(0, 0),
                MoveDirection.Bottom => new Point(0, height - width),
                MoveDirection.Right => new Point(height - width, 0),
            };

            var transitionBitmap = new Bitmap((int)fieldImageWidth, (int)fieldImageHeight);
            
            Graphics g = Graphics.FromImage(transitionBitmap);
            g.DrawImage(transitionImage, point);
            return transitionBitmap;
        }
        
        private static Bitmap GetImageAsBitmap(string path)
        {
            var uriSource = new Uri(path, UriKind.RelativeOrAbsolute);
            
            path = uriSource.ToString();
            
            Bitmap originalBmp = new Bitmap(path);

            Bitmap tempBitmap = new Bitmap(originalBmp.Width, originalBmp.Height);

            using Graphics g = Graphics.FromImage(tempBitmap);
            g.DrawImage(originalBmp, 0, 0);

            return tempBitmap;
        }
        
        private static Bitmap CreateImageTransition(FieldType primaryFt, FieldType secondaryFt)
        {
            var primaryFtSuccessful = GetCachedBitmap(GetCacheKey(primaryFt), out var primaryFtMap);

            if (!primaryFtSuccessful)
            {
                primaryFtMap = GetImageAsBitmap(PathToImage(primaryFt));
                SetCachedBitmap(GetCacheKey(primaryFt), primaryFtMap);
            }
            
            var secondaryFtSuccessful = GetCachedBitmap(GetCacheKey(secondaryFt), out var secondaryFtMap);

            if (!secondaryFtSuccessful)
            {
                secondaryFtMap = GetImageAsBitmap(PathToImage(secondaryFt));
                SetCachedBitmap(GetCacheKey(secondaryFt), secondaryFtMap);
            }
            
            var rand = new Random();
            var transitionBitmap = new Bitmap(primaryFtMap.Width / 9, primaryFtMap.Height);

            for (int x = 0; x < transitionBitmap.Width; x++)
            {
                for (int y = 0; y < transitionBitmap.Height; y++)
                {
                    transitionBitmap.SetPixel(x, y, rand.NextDouble() >= 0.2
                        ? primaryFtMap.GetPixel(x, y) : secondaryFtMap.GetPixel(x, y));
                }
            }

            return transitionBitmap;
        }

        public static BitmapImage FieldTypeToDefaultBitmapImage(FieldType fieldType)
        {
            if (GetCachedBitmapImage(GetCacheKey(fieldType), out var bitmapImage))
                return bitmapImage;

            if (GetCachedBitmap(GetCacheKey(fieldType), out var bitmap))
            {
                bitmapImage = bitmap.ToBitmapImage();
                SetCachedBitmapImage(GetCacheKey(fieldType), bitmapImage);
                return bitmapImage;
            }
            
            bitmap = GetImageAsBitmap($"./assets/{fieldType}.png");
            SetCachedBitmap(GetCacheKey(fieldType), bitmap);
            
            bitmapImage = bitmap.ToBitmapImage();
            SetCachedBitmapImage(GetCacheKey(fieldType), bitmapImage);

            return bitmapImage;
        }
        
        private static string GetCacheKey(FieldType ft1)
            => ft1.ToString();

        private static string GetCacheKey(FieldType ft1, FieldType ft2)
            =>  $"{ft1}-{ft2}";

        private static string GetCacheKey(MoveDirection moveDirection, FieldType ft1, FieldType ft2)
            => $"{moveDirection}-{ft1}-{ft2}";

        private static string GetCacheKey(FieldType ft1, FieldType ft2, FieldType ft3, FieldType ft4, FieldType ft5)
            => $"{ft1}-{ft2}-{ft3}-{ft4}-{ft5}";
    }
}