using System.Collections.Generic;
using MowingMachine.Services;

namespace MowingMachine.Models
{
    public class MowingStep
    {
        public MowingStep(Queue<MoveDirection> turns, MoveDirection moveDirection, FieldType fieldType)
        {
            Turns = turns;
            MoveDirection = moveDirection;
            FieldType = fieldType;
            TotalEnergyExpense = Constants.TranslateMoveToExpense(fieldType) + turns.Count * Constants.TurnExpense;
        }

        public Queue<MoveDirection> Turns { get; }
        public FieldType FieldType { get; }
        public MoveDirection MoveDirection { get; }
        public double TotalEnergyExpense { get; }
    }
}