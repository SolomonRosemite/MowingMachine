using System;
using MowingMachine.Models;

namespace MowingMachine.Services
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
                var (addX, addY) = MowingMachineService.TranslateDirection(direction);

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
            var (addX, addY) = MowingMachineService.TranslateDirection(direction);
            x += addX;
            y += addY;

            return (x, y);
        }
        
        public static Coordinate GetTranslatedCoordinate(this Coordinate coordinate, MoveDirection direction)
        {
            var (addX, addY) = MowingMachineService.TranslateDirection(direction);

            return new Coordinate(coordinate.X + addX, coordinate.Y + addY);;
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
    }
}