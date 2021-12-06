using System.Collections.Generic;
using System.Linq;
using MowingMachine.Models;

namespace MowingMachine.Common
{
    public static class Constants
    {
        private static readonly Dictionary<FieldType, double> ExpensesDictionary = new()
        {
            { FieldType.Grass, 10 },
            { FieldType.ChargingStation, 10 },
            { FieldType.MowedLawn, 10 },
            
            { FieldType.CobbleStone, 5 },
            { FieldType.Sand, 20 },
        };

        public static double TurnExpense => 2;
        
        public static double TranslateMoveToExpense(FieldType fieldType)
        {
            return ExpensesDictionary.First(e => e.Key == fieldType).Value;
        }
    }
}