using System;
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

        public int X { get; private set; }
        public int Y { get; private set; }

        public Offset Add(Offset offset) => new(X + offset.X, Y + offset.Y);
        public Offset Subtract(Offset offset) => new(X - offset.X, Y - offset.Y);
        public Offset Add(int addX, int addY) => new(X + addX, Y + addY);

        private bool CompareTo(Offset offset) => X == offset.X && Y == offset.Y;
                
        public void UpdateOffset(Offset offset)
        {
            X = offset.X;
            Y = offset.Y;
        }
        
        public override string ToString() => $"{X}-{Y}";

        public override bool Equals(object obj)
        {
            return obj is Offset item && CompareTo(item);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X.GetHashCode(), Y.GetHashCode());
        }

        public static bool operator ==(Offset obj1, Offset obj2) => CompareTo(obj1, obj2);

        public static bool operator !=(Offset obj1, Offset obj2) => !CompareTo(obj1, obj2);

        private static bool CompareTo(Offset obj1, Offset obj2)
        {
            if (obj1 is null && obj2 is null)
                return true;
            
            if (obj1 is null || obj2 is null)
                return false;
            
            return obj1.CompareTo(obj2);
        }
    }
}