using System;
using System.Collections.Generic;
using System.Linq;
using MowingMachine.Models;

namespace MowingMachine.Common
{
    public static class ExtensionMethods
    {
        public static int GetField(this int[][] map, int x, int y)
        {
            if (x < 0 || y < 0 || x == map.Length || y == map.Length)
                return -1;
         
            return map[x][y];
        }
        
        public static int GetTranslatedField(this int[][] map, int x, int y, MoveDirection direction)
        {
            try
            {
                var (addX, addY) = direction.TranslateDirection();

                x += addX;
                y += addY;

                return map[x][y];
            }
            catch
            {
                return -1;
            }
        }
        
        public static (int, int) GetTranslatedCoordinate(this int[][] _, int x, int y, MoveDirection direction)
        {
            var (addX, addY) = direction.TranslateDirection();
            x += addX;
            y += addY;

            return (x, y);
        }
        
        public static bool AreNeighbors(this Offset o1, Offset o2)
        {
            return Math.Abs(o1.X - o2.X) + Math.Abs(o1.Y - o2.Y) <= 1;
        }
        
        public static MoveDirection InvertDirection(this MoveDirection direction)
        {
            return direction switch
            {
                MoveDirection.Top => MoveDirection.Bottom,
                MoveDirection.Right => MoveDirection.Left,
                MoveDirection.Bottom => MoveDirection.Top,
                MoveDirection.Left => MoveDirection.Right,
                _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
            };
        }
        
        public static MoveDirection TranslateOffsetToDirection(this Offset offset)
        {
            if (offset.CompareTo(new Offset(0, 1)))
                return MoveDirection.Top;
            if (offset.CompareTo(new Offset(1, 0)))
                return MoveDirection.Right;
            if (offset.CompareTo(new Offset(0, -1)))
                return MoveDirection.Bottom;
            if (offset.CompareTo(new Offset(-1, 0)))
                return MoveDirection.Left;

            throw new ArgumentException("Offset was beyond");
        }

        private static (int, int) TranslateDirection(this MoveDirection direction)
        {
            return direction switch
            {
                MoveDirection.Top => (1, 0),
                MoveDirection.Left => (0, -1),
                MoveDirection.Right => (0, 1),
                MoveDirection.Bottom => (-1, 0),
            };
        }
        
        public static (int, int) TranslateDirectionToOffset(this MoveDirection direction)
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
        
        public static void Move<T>(this List<T> list, T item, int newIndex)
        {
            if (item != null)
            {
                var oldIndex = list.IndexOf(item);
                if (oldIndex > -1)
                {
                    list.RemoveAt(oldIndex);

                    if (newIndex > oldIndex) newIndex--;
                    // the actual index could have shifted due to the removal

                    list.Insert(newIndex, item);
                }
            }
        }
        
        public static int[][] DeepClone(this int[][] value)
        {
            return value.Select(a => a.ToArray()).ToArray();
        }
    }
}