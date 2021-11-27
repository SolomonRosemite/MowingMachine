using System;
using System.Collections.Generic;
using System.Windows.Documents;
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

        private int[][] Map { get; }
        
        public FieldType MoveMowingMachine(MoveDirection direction, FieldType previousField)
        {
            var (mowingMachineX, mowingMachineY) = GetMowingMachineCoordinate();
            var (x, y) = Map.GetTranslatedCoordinate(mowingMachineX, mowingMachineY, direction);

            var nextPreviousField = (FieldType) Map[x][y];
            
            Map[mowingMachineX][mowingMachineY] = (int) previousField;
            Map[x][y] = (int) FieldType.MowingMachine;

            OnUpdateMap?.Invoke(this, new OnUpdateMapEventArgs {Map = (int[][]) this.Map.Clone()});

            return nextPreviousField;
        }

        public List<Coordinate> GetAllReachableCoordinates()
        {
            var reachableCoordinates = new List<Coordinate>();
            var (mowingMachineX, mowingMachineY) = GetMowingMachineCoordinate();

            // Todo: Go trough each item in the map. Starting from [0, 0] then [0, 1] then [1, 0] and last [1, 1]. And continue this pattern.
            // For each item we use dijkstra's algorithm to see if we can even get to the mowing machine. If so, we add it to the list. If not we dont.
            // https://en.wikipedia.org/wiki/Dijkstra%27s_algorithm

            return reachableCoordinates;
        }
        
        public Coordinate GetNextTarget(Coordinate reachableCoordinate)
        {
            // Todo: Here we use dijkstra's algorithm to find the path from the mowing machine to the next grass field in the reachable list.

            throw new NotImplementedException();
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
    }
}