using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using MowingMachine.Models;

namespace MowingMachine.Services
{
    public static class MowingMachineService
    {
        public static (ColumnDefinition[], RowDefinition[]) GenerateDefinitions(int columns, int rows)
        {
            var columnDefinitions = new ColumnDefinition [columns];
            var rowDefinitions = new RowDefinition [rows];
            
            for (int i = 0; i < columns; i++)
                columnDefinitions[i] = new ColumnDefinition {Width = new GridLength(1, GridUnitType.Star)};
            
            for (int i = 0; i < rows; i++)
                rowDefinitions[i] = new RowDefinition {Height = new GridLength(1, GridUnitType.Star)};

            return (columnDefinitions, rowDefinitions);
        }

        public static IEnumerable<UIElement> GetUiElements(ColumnDefinition[] columnDefinitions, RowDefinition[] rowDefinitions)
        {
            var elements = new UIElement[columnDefinitions.Length * rowDefinitions.Length];

            int count = 0;
            for (int x = 0; x < columnDefinitions.Length; x++)
            {
                for (int y = 0; y < rowDefinitions.Length; y++)
                {
                    var rectangle = new Rectangle
                    {
                        Fill = new SolidColorBrush(Colors.Green),
                        Stroke = new SolidColorBrush(Colors.Black),
                        StrokeThickness = 1,
                        Tag = FieldType.Grass,
                    };
                    
                    Grid.SetColumn(rectangle, y);
                    Grid.SetRow(rectangle, x);

                    elements[count++] = rectangle;
                }
            }

            return elements;
        }

        private static UIElement FieldTypeToItem(FieldType fieldType)
        {
            return fieldType switch
            {
                FieldType.Sand => new Rectangle
                {
                    Fill = new SolidColorBrush(Colors.SandyBrown),
                    Stroke = new SolidColorBrush(Colors.Black),
                    StrokeThickness = 1,
                    Tag = fieldType,
                },
                FieldType.Grass => new Rectangle
                {
                    Fill = new SolidColorBrush(Colors.Green),
                    Stroke = new SolidColorBrush(Colors.Black),
                    StrokeThickness = 1,
                    Tag = fieldType,
                },
                FieldType.MowedLawn => new Rectangle
                {
                    Fill = new SolidColorBrush(Colors.SeaGreen),
                    Stroke = new SolidColorBrush(Colors.Black),
                    StrokeThickness = 1,
                    Tag = fieldType,
                },
                FieldType.Cobbled => new Rectangle
                {
                    Fill = new SolidColorBrush(Colors.DimGray),
                    Stroke = new SolidColorBrush(Colors.Black),
                    StrokeThickness = 1,
                    Tag = fieldType,
                },
                FieldType.ChargingStation => new Rectangle
                {
                    Fill = new SolidColorBrush(Colors.MediumPurple),
                    Stroke = new SolidColorBrush(Colors.Black),
                    StrokeThickness = 1,
                    Tag = fieldType,
                },
                _ => throw new ArgumentOutOfRangeException(nameof(fieldType), fieldType, null),
            };
        }

        private static FieldType NumberToFieldType(double value)
        {
            var items = Enum.GetValues<FieldType>();
            value /= items.Length;

            double c = 1 / (double)items.Length;
            for (int i = 0; i < items.Length; i++)
            {
                if (NumberIsBetween(value, i * c, (i + 1) * c))
                {
                    
                }
            }

            throw new Exception($"Shouldn't get here. Value was {value}");
        }

        private static void GenerateNoise()
        {
            
        }

        private static double DoMath()
        {
            SimplexNoise.Noise.Calc2D(x, x, 1f);
        }

        private static bool NumberIsBetween(double numberToCheck, double bottom, double top)
        {
            return numberToCheck >= bottom && numberToCheck <= top;
        }
    }
}