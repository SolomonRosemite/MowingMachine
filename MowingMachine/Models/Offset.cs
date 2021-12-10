using MowingMachine.Common;

namespace MowingMachine.Models
{
    public class Offset
    {
        public Offset(int x, int y)
        {
            X = x;
            Y = y;

            if (X == -6 && y == 3)
            {
                
            }
        }
        
        public Offset(MoveDirection direction)
        {
            var (x, y) = direction.TranslateDirectionToOffset();
            
            X = x;
            Y = y;
            
            if (X == -6 && y == 3)
            {
                
            }
        }

        public int X { get; private set;  }
        public int Y { get; private set; }

        public Offset Add(Offset offset) => new(X + offset.X, Y + offset.Y);
        public Offset Subtract(Offset offset) => new(X - offset.X, Y - offset.Y);
        public Offset Add(int addX, int addY) => new(X + addX, Y + addY);

        public bool CompareTo(Offset offset) => X == offset.X && Y == offset.Y;
        
        public override string ToString() => $"{X}-{Y}";

        public void UpdateOffset(Offset offset)
        {
            X = offset.X;
            Y = offset.Y;
        }
    }
}