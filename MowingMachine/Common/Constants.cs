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
    }
}