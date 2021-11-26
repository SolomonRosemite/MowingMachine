using System;
using MowingMachine.Services;

namespace MowingMachine.Models
{
    public class MapManager
    {
        public event EventHandler<OnUpdateMapEventArgs> OnUpdateMap;

        public class OnUpdateMapEventArgs : EventArgs
        {
            public int[][] Map;
        }
        
        public MapManager(int[][] map)
        {
            this.Map = map;
        }

        public int[][] Map { get; private set;  }
        
        public void MoveMowingMachine(MoveDirection direction, FieldType previousField)
        {
            var (mowingMachineX, mowingMachineY) = GetMowingMachineCoordinate();
            var (x, y) = Map.GetTranslatedCoordinate(mowingMachineX, mowingMachineY, direction);

            Map[mowingMachineX][mowingMachineY] = (int) previousField;
            Map[x][y] = (int) FieldType.MowingMachine;

            OnUpdateMap?.Invoke(this, new OnUpdateMapEventArgs {Map = (int[][]) this.Map.Clone()});
        }

        public int[][] GetFieldsInSight()
        {
            var (x, y) = GetMowingMachineCoordinate();
            return GetFieldsAroundCoordinate(x, y);
        }

        private (int, int) GetMowingMachineCoordinate()
        {
            for (int x = 0; x < Map.Length; x++)
            {
                for (int y = 0; y < Map.Length; y++)
                {
                    FieldType type = (FieldType)Map[x][y];

                    if (type == FieldType.MowingMachine)
                        return (x, y);
                }
            }

            throw new Exception("Could not find Mowing Machine.");
        }

        private int[][] GetFieldsAroundCoordinate(int xCoordinate, int yCoordinate)
        {
            var map = new[]
            {
                new int[3],
                new int[3],
                new int[3],
            };

            int mapX = 0;
            for (int x = xCoordinate - 1; x < xCoordinate + 2; x++)
            {
                int mapY = 0;

                for (int y = yCoordinate - 1; y < yCoordinate + 2; y++)
                {
                    map[mapX][mapY] = Map.GetField(x, y);
                    mapY++;
                }

                mapX++;
            }
            return map;
        }
    }
}