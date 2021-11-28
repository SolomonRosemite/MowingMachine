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
                }
            }
            
            return reachableCoordinates;
        }

        
        private List<Coordinate> PathToGoalCoordinate(Coordinate start, Coordinate goal)
        {
            
            var visitedCoordinates = new Dictionary<string, Coordinate>();
            var nextCoordinatesToVisit = new Queue<CoordinateInfo>();
            
            nextCoordinatesToVisit.Enqueue(new CoordinateInfo(start, null));

            // int count = 0;
            while (nextCoordinatesToVisit.Count != 0)
            {
                var cellInfo = nextCoordinatesToVisit.Dequeue();

                if (FoundSearchingCell(cellInfo))
                    break;

                // if (count++ > 1000)
                //     count = 0;
                // Console.WriteLine("Test");
            }
            
            var tracedPath = new List<Coordinate>();

            var currenCoordinate = goal;
            while (visitedCoordinates.Any(c => c.CurrentCoordinate.X == currenCoordinate.X && c.CurrentCoordinate.Y == currenCoordinate.Y))
            {
                var coord = visitedCoordinates.FirstOrDefault(c => c.CurrentCoordinate.X == currenCoordinate.X && c.CurrentCoordinate.Y == currenCoordinate.Y);

                if (coord == null)
                    break;
                
                tracedPath.Add(currenCoordinate);
                currenCoordinate = coord.PrevCoordinate;
                visitedCoordinates.Remove(coord);
            }
            
            return tracedPath;

            bool FoundSearchingCell(CoordinateInfo info)
            {
                if (!IsValidField(info.CurrentCoordinate))
                    return false;

                // If it already exists, dont add again
                if (visitedCoordinates.Any(c => c != null && c.CurrentCoordinate.X == info.CurrentCoordinate.X && c.CurrentCoordinate.Y == info.CurrentCoordinate.Y))
                   return false;
                   
                visitedCoordinates.Add(new CoordinateInfo(info.CurrentCoordinate, info.PrevCoordinate));

                if (info.CurrentCoordinate.X == goal.X && info.CurrentCoordinate.Y == goal.Y)
                    return true;
            
                nextCoordinatesToVisit.Enqueue(new CoordinateInfo(info.CurrentCoordinate.X, info.CurrentCoordinate.Y + 1, info.PrevCoordinate));
                nextCoordinatesToVisit.Enqueue(new CoordinateInfo(info.CurrentCoordinate.X, info.CurrentCoordinate.Y - 1, info.PrevCoordinate));
                nextCoordinatesToVisit.Enqueue(new CoordinateInfo(info.CurrentCoordinate.X + 1, info.CurrentCoordinate.Y, info.PrevCoordinate));
                nextCoordinatesToVisit.Enqueue(new CoordinateInfo(info.CurrentCoordinate.X - 1, info.CurrentCoordinate.Y, info.PrevCoordinate));
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