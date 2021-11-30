using System.Collections.Generic;

namespace MowingMachine.Models
{
    public class MowingStep
    {
        public MowingStep(Queue<MoveDirection> turns, MoveDirection moveDirection, double totalEnergyExpense)
        {
            Turns = turns;
            MoveDirection = moveDirection;
            TotalEnergyExpense = totalEnergyExpense;
        }

        public Queue<MoveDirection> Turns { get; }
        public MoveDirection MoveDirection { get; }
        public double TotalEnergyExpense { get; }
    }
}