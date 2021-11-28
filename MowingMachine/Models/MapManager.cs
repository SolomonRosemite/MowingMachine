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

            for (int x = 0; x < Map.Length; x++)
            {
                for (int y = 0; y < Map.Length; y++)
                {
                    var coordinate = new Coordinate(x, y);
                    
                    var isReachable = PathToGoalCoordinate(coordinate, mowingMachineCoordinate) != null;

                    if (isReachable)
                    {
                        reachableCoordinates.Add(coordinate);
                    }
                    else
                    {
                        
                    }
                }
            }
            
            return reachableCoordinates;
        }

        
        private List<Coordinate> PathToGoalCoordinate(Coordinate start, Coordinate goal)
        {
            if (!IsValidField(start))
                return null;
            
            var visitedCoordinates = new Dictionary<string, Coordinate>();
            var nextCoordinatesToVisit = new Queue<CoordinateInfo>();
            // var nextCoordinatesToVisit = new Queue<CoordinateInfo>();
            
            nextCoordinatesToVisit.Enqueue(new CoordinateInfo(start, null));

            // int count = 0;
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
            
                // nextCoordinatesToVisit.Enqueue(new Coordinate(info.CurrentCoordinate.X, info.CurrentCoordinate.Y + 1));
                // nextCoordinatesToVisit.Enqueue(new Coordinate(info.CurrentCoordinate.X, info.CurrentCoordinate.Y - 1));
                // nextCoordinatesToVisit.Enqueue(new Coordinate(info.CurrentCoordinate.X + 1, info.CurrentCoordinate.Y));
                // nextCoordinatesToVisit.Enqueue(new Coordinate(info.CurrentCoordinate.X - 1, info.CurrentCoordinate.Y));
                nextCoordinatesToVisit.Enqueue(new CoordinateInfo(info.CurrentCoordinate.X, info.CurrentCoordinate.Y + 1, info.CurrentCoordinate));
                nextCoordinatesToVisit.Enqueue(new CoordinateInfo(info.CurrentCoordinate.X, info.CurrentCoordinate.Y - 1, info.CurrentCoordinate));
                nextCoordinatesToVisit.Enqueue(new CoordinateInfo(info.CurrentCoordinate.X + 1, info.CurrentCoordinate.Y, info.CurrentCoordinate));
                nextCoordinatesToVisit.Enqueue(new CoordinateInfo(info.CurrentCoordinate.X - 1, info.CurrentCoordinate.Y, info.CurrentCoordinate));
                return false;
            }

            bool IsValidField(Coordinate coordinate)
            {
                try
                {
                    var value = Map[coordinate.X][coordinate.Y];
                    return value != -1 && (FieldType) value != FieldType.Water;
                }
                catch
                {
                    return false;
                }
            }
        }
        
        public Coordinate GetNextTarget(Coordinate reachableCoordinate)
        {
            // Todo: Here we use dijkstra's algorithm to find the path from the mowing machine to the next grass field in the reachable list.

            throw new NotImplementedException();
        }
        
        private Coordinate GetMowingMachineCoordinate()
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