using System;
using System.Collections.Generic;
using MowingMachine.Models;

namespace MowingMachine.Common
{
    public class FieldComparer : IEqualityComparer<Field>
    {
        public bool Equals(Field x, Field y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return x.Offset.X == y.Offset.X && x.Offset.Y == y.Offset.Y;
        }

        public int GetHashCode(Field obj)
        {
            return HashCode.Combine(obj.Offset.X, obj.Offset.Y);
        }
    }
}