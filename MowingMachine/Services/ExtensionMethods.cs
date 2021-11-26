using MowingMachine.Models;

namespace MowingMachine.Services
{
    public static class ExtensionMethods
    {
        public static int GetField(this int[][] map, int x, int y)
        {
            try
            {
                return map[x][y];
            }
            catch
            {
                return -1;
            }
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
    }
}