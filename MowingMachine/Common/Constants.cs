using System.Collections.Generic;
using System.Linq;
using MowingMachine.Models;

namespace MowingMachine.Common
{
    public static class Constants
    {
        private static readonly Dictionary<FieldType, int> _ExpensesDictionary = new()
        {
            { FieldType.Grass, 10 },
            { FieldType.ChargingStation, 10 },
            { FieldType.MowedLawn, 10 },
            
            { FieldType.CobbleStone, 5 },
            { FieldType.Sand, 20 },
        };

        public static double TurnExpense => 4;
        
        public static double TranslateMoveToExpense(FieldType fieldType)
        {
            return _ExpensesDictionary.First(e => e.Key == fieldType).Value;
        }

        public static int[][] DefaultMapSample
        {
            get
            {
                int[][] sample =
                {
                    // Todo: Revert later.
                    new[] {6, 1, 1, 1, 1, 1, 1, 6, 6, 6},
                    new[] {1, 1, 6, 6, 6, 1, 1, 1, 6, 6},
                    new[] {1, 1, 6, 6, 6, 6, 1, 6, 1, 6},
                    new[] {1, 1, 6, 6, 6, 6, 1, 1, 0, 1},
                    new[] {1, 1, 1, 6, 1, 3, 1, 1, 1, 1},
                    new[] {1, 6, 1, 6, 1, 3, 6, 1, 1, 1},
                    new[] {6, 1, 1, 6, 1, 3, 1, 1, 5, 1},
                    new[] {1, 0, 6, 3, 3, 6, 1, 0, 0, 0},
                    new[] {1, 6, 3, 1, 6, 1, 1, 0, 6, 0},
                    new[] {6, 1, 3, 1, 6, 1, 0, 0, 0, 6},
                    // new[] {1, 1, 1, 1, 1, 1, 1, 6, 6, 6},
                    // new[] {1, 1, 6, 6, 6, 1, 1, 1, 1, 6},
                    // new[] {1, 1, 6, 6, 6, 6, 1, 1, 1, 6},
                    // new[] {1, 1, 6, 6, 6, 6, 1, 1, 1, 1},
                    // new[] {1, 1, 1, 1, 1, 3, 1, 1, 1, 1},
                    // new[] {1, 1, 1, 1, 1, 3, 1, 1, 1, 1},
                    // new[] {1, 1, 1, 1, 1, 3, 1, 1, 5, 1},
                    // new[] {1, 6, 3, 3, 3, 3, 1, 0, 0, 0},
                    // new[] {1, 1, 3, 1, 1, 1, 1, 0, 6, 0},
                    // new[] {1, 1, 3, 1, 1, 1, 0, 0, 0, 0},
                };

                return sample;
            }
        }
    }
}