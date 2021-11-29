using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
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
            var mowingMachineCoordinate = GetMowingMachineCoordinate();
            var (x, y) = Map.GetTranslatedCoordinate(mowingMachineCoordinate.X, mowingMachineCoordinate.Y, direction);

            var nextPreviousField = (FieldType) Map[x][y];
            
            Map[mowingMachineCoordinate.X][mowingMachineCoordinate.Y] = (int) previousField;
            Map[x][y] = (int) FieldType.MowingMachine;

            OnUpdateMap?.Invoke(this, new OnUpdateMapEventArgs {Map = (int[][]) this.Map.Clone()});

            return nextPreviousField;
        }

        public List<Coordinate> GetAllReachableCoordinates()
        {
            var reachableCoordinates = new List<Coordinate>();
            var mowingMachineCoordinate = GetMowingMachineCoordinate();

            int x = 0;
            while (x < Map.Length)
            {
                int y = x % 2 == 0 ? 0 : 9;

                if (y == 0)
                {
                    while (y < Map.Length)
                    {
                        var coordinate = new Coordinate(x, y);
                        
                        var isReachable = PathToGoalCoordinate(coordinate, mowingMachineCoordinate) != null;

                        if (isReachable)
                        {
                            reachableCoordinates.Add(coordinate);
                        }

                        y++;
                    }
                }
                else
                {
                    while (y >= 0)
                    {
                        var coordinate = new Coordinate(x, y);
                        
                        var isReachable = PathToGoalCoordinate(coordinate, mowingMachineCoordinate) != null;

                        if (isReachable)
                        {
                            reachableCoordinates.Add(coordinate);
                        }

                        y--;
                    }
                }

                x++;
            }
            
            // reachableCoordinates.ForEach(c => Map[c.X][c.Y] = 2);
            
            return reachableCoordinates;
        }

        
        public List<Coordinate> PathToGoalCoordinate(Coordinate start, Coordinate goal, bool ignoreInitialPoint = false)
        {
            if (!ignoreInitialPoint && !IsMowable(start.X, start.Y))
                return null;
            
            var visitedCoordinates = new Dictionary<string, Coordinate>();
            var nextCoordinatesToVisit = new Queue<CoordinateInfo>();
            
            nextCoordinatesToVisit.Enqueue(new CoordinateInfo(start, null));

            while (nextCoordinatesToVisit.Count != 0)
            {
                var cellInfo = nextCoordinatesToVisit.Dequeue();

                if (FoundSearchingCell(cellInfo))
                    break;
            }
            
            var tracedPath = new List<Coordinate>();

            var currenCoordinate = goal;
            while (visitedCoordinates.TryGetValue(currenCoordinate.ToString(), out var coord))
            {
                if (coord == null)
                    break;
                
                tracedPath.Add(currenCoordinate);
                currenCoordinate = coord;
            }
            
            return tracedPath;

            bool FoundSearchingCell(CoordinateInfo info)
            {
                if (!IsValidField(info.CurrentCoordinate))
                    return false;

                // If it already exists, dont add again
                if (visitedCoordinates.ContainsKey(info.CurrentCoordinate.ToString()))
                   return false;

                visitedCoordinates[info.CurrentCoordinate.ToString()] = info.PrevCoordinate;

                if (info.CurrentCoordinate.X == goal.X && info.CurrentCoordinate.Y == goal.Y)
                    return true;
            
                nextCoordinatesToVisit.Enqueue(new CoordinateInfo(info.CurrentCoordinate.X - 1, info.CurrentCoordinate.Y, info.CurrentCoordinate));
                nextCoordinatesToVisit.Enqueue(new CoordinateInfo(info.CurrentCoordinate.X, info.CurrentCoordinate.Y + 1, info.CurrentCoordinate));
                nextCoordinatesToVisit.Enqueue(new CoordinateInfo(info.CurrentCoordinate.X, info.CurrentCoordinate.Y - 1, info.CurrentCoordinate));
                nextCoordinatesToVisit.Enqueue(new CoordinateInfo(info.CurrentCoordinate.X + 1, info.CurrentCoordinate.Y, info.CurrentCoordinate));
                return false;
            }

            bool IsValidField(Coordinate coordinate)
            {
                // In case coordinate is out of bounds
                if (coordinate.X < 0 || coordinate.Y < 0 || coordinate.X == Map.Length || coordinate.Y == Map.Length )
                    return false;
                
                var value = Map[coordinate.X][coordinate.Y];
                return value != -1 && (FieldType) value is not FieldType.Water;
                // return value != -1 && (FieldType) value is FieldType.Grass;
            }

            bool IsMowable(int x, int y) => Map[x][y] == 1;
        }
        
        public Coordinate GetMowingMachineCoordinate()
        {
            for (int x = 0; x < Map.Length; x++)
            {
                for (int y = 0; y < Map.Length; y++)
                {
                    FieldType type = (FieldType)Map[x][y];

                    if (type == FieldType.MowingMachine)
                        return new Coordinate(x, y);
                }
            }

            throw new Exception("Could not find Mowing Machine.");
        }
    }
}