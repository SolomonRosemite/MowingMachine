using System;

namespace MowingMachine.Models
{
    public class Offset
    {
        public Offset(int x, int y)
        {
            X = x;
            Y = y;
        }
        
        public Offset(MoveDirection direction)
        {
            var (x, y) = DirectionToOffset(direction);
            
            X = x;
            Y = y;
        }

        public int X { get; }
        public int Y { get; }

        public Offset Add(Offset offset) => new(X + offset.X, Y + offset.Y);
        public Offset Add(int addX, int addY) => new(X + addX, Y + addY);

        private static (int, int) DirectionToOffset(MoveDirection direction)
        {
            return direction switch
            {
                MoveDirection.Top => (0, 1),
                MoveDirection.Right => (1, 0),
                MoveDirection.Bottom => (0, -1),
                MoveDirection.Left => (-1, 0),
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
            };
        }
    }
}