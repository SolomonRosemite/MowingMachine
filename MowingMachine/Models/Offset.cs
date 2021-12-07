using MowingMachine.Common;

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
            var (x, y) = direction.TranslateDirectionToOffset();
            
            X = x;
            Y = y;
        }

        public int X { get; }
        public int Y { get; }

        public Offset Add(Offset offset) => new(X + offset.X, Y + offset.Y);
        public Offset Subtract(Offset offset) => new(X - offset.X, Y - offset.Y);
        public Offset Add(int addX, int addY) => new(X + addX, Y + addY);

        public bool CompareTo(Offset offset) => X == offset.X && Y == offset.Y;
        
        public override string ToString() => $"{X}-{Y}";
    }
}