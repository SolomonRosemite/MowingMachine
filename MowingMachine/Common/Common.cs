using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MowingMachine.Models;

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
            for (int x = 0; x < mapSample.Length; x++)
            {
                for (int y = 0; y < mapSample.Length; y++)
                {
                    FieldType type = (FieldType)mapSample[x][y];
                
                    var element = new Button
                    {
                        Content = new Image
                        {
                            Source = FieldTypeToItem(type),
                            Stretch = Stretch.Fill,
                            Tag = type,
                        },
                    };
                
                    Grid.SetRow(element, x);
                    Grid.SetColumn(element, y);

                    elements[count++] = element;
                }
            }
            
            return elements;
        }

        private static BitmapImage FieldTypeToItem(FieldType fieldType)
        {
            return fieldType switch
            {
                FieldType.Sand => GetImage("./assets/sand.png"),
                FieldType.Grass => GetImage("./assets/grass.png"),
                FieldType.MowedLawn => GetImage("./assets/mowed_lawn.png"),
                FieldType.CobbleStone => GetImage("./assets/cobblestone.png"),
                FieldType.ChargingStation => GetImage("./assets/charging_station.png"),
                FieldType.MowingMachine => GetImage("./assets/mowing_machine.png"),
                FieldType.Water => GetImage("./assets/water.png"),
                _ => throw new ArgumentOutOfRangeException(nameof(fieldType), fieldType, null),
            };
        }

        private static BitmapImage GetImage(string path)
        {
            var uriSource = new Uri(path, UriKind.RelativeOrAbsolute);
            var image = new BitmapImage(uriSource);

            if (image.Height < -1) { }
            
            return image;
        }
    }
}