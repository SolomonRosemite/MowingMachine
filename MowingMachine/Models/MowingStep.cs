using System.Collections.Generic;
using MowingMachine.Common;

namespace MowingMachine.Models
{
    public class MowingStep
    {
        public MowingStep(Queue<MoveDirection> turns, MoveDirection moveDirection, FieldType fieldType, bool ignoreMowingExpense = false)
        {
            Turns = turns;
            MoveDirection = moveDirection;
            FieldType = fieldType;
            var expense = Constants.TranslateMoveToExpense(fieldType) + turns.Count * Constants.TurnExpense;

            // If the field type is grass or cobble the mowing machine will mow it. Thus use more energy.
            if (!ignoreMowingExpense && fieldType is FieldType.Grass or FieldType.CobbleStone)
                expense += 5;
            
            TotalEnergyExpense = expense;
        }

        public Queue<MoveDirection> Turns { get; }
        public FieldType FieldType { get; }
        public MoveDirection MoveDirection { get; }
        public double TotalEnergyExpense { get; }
    }
}