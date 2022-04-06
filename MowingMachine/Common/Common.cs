using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MowingMachine.Models;
using Image = System.Windows.Controls.Image;

namespace MowingMachine.Common
{
    public static class Common
    {
        public static (ColumnDefinition[], RowDefinition[]) GenerateDefinitions(int columns, int rows)
        {
            var columnDefinitions = new ColumnDefinition [columns];
            var rowDefinitions = new RowDefinition [rows];
            
            for (int i = 0; i < columns; i++)
                columnDefinitions[i] = new ColumnDefinition {Width = new GridLength(1, GridUnitType.Star), };
            
            for (int i = 0; i < rows; i++)
                rowDefinitions[i] = new RowDefinition {Height = new GridLength(1, GridUnitType.Star)};

            return (columnDefinitions, rowDefinitions);
        }

        public static IEnumerable<UIElement> GetUiElements(int[][] mapSample)
        {
            var elements = new UIElement[(int)Math.Pow(mapSample.Length, 2)];

            int count = 0;
            for (int y = 0; y < mapSample.Length; y++)
            {
                for (int x = 0; x < mapSample.Length; x++)
                {
                    FieldType type = (FieldType)mapSample[y][x];
                    FieldType fieldTop = mapSample.GetFieldInvertedCasted(x, y - 1);
                    FieldType fieldBottom = mapSample.GetFieldInvertedCasted(x, y + 1);
                    FieldType fieldLeft = mapSample.GetFieldInvertedCasted(x - 1, y);
                    FieldType fieldRight = mapSample.GetFieldInvertedCasted(x + 1, y);

                    var element = new Image
                    {
                        Source = MapUtilities.GetFieldBitmapWithTransition(type, fieldTop, fieldRight, fieldBottom, fieldLeft),
                        Stretch = Stretch.Fill,
                        Tag = type,
                    };
                    
                    Grid.SetRow(element, y);
                    Grid.SetColumn(element, x);

                    elements[count++] = element;
                }
            }
            
            return elements;
        }

        public static UIElement GetUiElement(int[][] _, int fieldType, int x, int y)
        {
            FieldType type = (FieldType)fieldType;
            
            var element = new Image
            {
                Source = MapUtilities.FieldTypeToDefaultBitmapImage(type),
                Stretch = Stretch.Fill,
                Tag = type,
            };
            
            Grid.SetRow(element, y);
            Grid.SetColumn(element, x);

            return element;
        }
        
        private static void Save(BitmapSource image)
        {
            var encoder = new JpegBitmapEncoder();
            var photoId = Guid.NewGuid();
            var photoLocation = photoId + ".png";  //file name 

            encoder.Frames.Add(BitmapFrame.Create(image));

            using var filestream = new FileStream(photoLocation, FileMode.Create);
            encoder.Save(filestream);
        } 
    }
}