using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading;
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
        private MowingStep _currentlyWorkingMowingStep;
        
        public FieldType MoveMowingMachine(MowingStep step, FieldType previousField)
        {
            _currentlyWorkingMowingStep = step;
            
            var mowingMachineCoordinate = GetMowingMachineCoordinate();
            var (x, y) = Map.GetTranslatedCoordinate(mowingMachineCoordinate.X, mowingMachineCoordinate.Y, step.MoveDirection);

            var nextPreviousField = (FieldType) Map[x][y];
            
            Map[mowingMachineCoordinate.X][mowingMachineCoordinate.Y] = (int) previousField;
            Map[x][y] = (int) FieldType.MowingMachine;

            return nextPreviousField;
        }

        private void Update()
        {
            OnUpdateMap?.Invoke(this, new OnUpdateMapEventArgs { Map = (int[][]) this.Map.Clone() });
        }

        public bool Verify()
        {
            if (_currentlyWorkingMowingStep is null)
                return true;
            
            if (_currentlyWorkingMowingStep.Turns.Count != 0)
            {
                var moveDirection = _currentlyWorkingMowingStep.Turns.Dequeue();
                Console.WriteLine($"Turned mowing machine in direction: {moveDirection}");
            }
            else
            {
                Update();
                _currentlyWorkingMowingStep = null;
            }

            return false;
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

                if (GetNeighborCells(cellInfo))
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

            bool GetNeighborCells(CoordinateInfo info)
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
        
        public FieldOfView GetFieldsOfView()
        {
            var coordinate = GetMowingMachineCoordinate();
            return new FieldOfView(GetFieldsAroundCoordinate(coordinate.X, coordinate.Y));
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