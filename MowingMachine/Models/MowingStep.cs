using System.Collections.Generic;
using MowingMachine.Common;

namespace MowingMachine.Models
{
    public class MowingStep
    {
        // Todo: Revert
        public MowingStep(Queue<MoveDirection> turns, MoveDirection moveDirection, FieldType fieldType, Offset finalPos)
        // public MowingStep(Queue<MoveDirection> turns, MoveDirection moveDirection, FieldType fieldType)
        {
            Turns = turns;
            FinalPos = finalPos;
            MoveDirection = moveDirection;
            FieldType = fieldType;
            var expense = Constants.TranslateMoveToExpense(fieldType) + turns.Count * Constants.TurnExpense;

            // If the field type is grass or cobble the mowing machine will mow it. Thus use more energy.
            if (fieldType is FieldType.Grass or FieldType.CobbleStone)
                expense += 5;
            
            TotalEnergyExpense = expense;
        }

        public Offset FinalPos { get; }
        public Queue<MoveDirection> Turns { get; }
        public FieldType FieldType { get; }
        public MoveDirection MoveDirection { get; }
        public double TotalEnergyExpense { get; }
    }
}