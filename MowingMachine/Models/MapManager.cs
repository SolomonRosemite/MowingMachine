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
                Console.WriteLine($"Turned mowing machine in direction: {moveDirection}.");
            }
            else
            {
                Console.WriteLine($"Mowing machine moving forward.");
                Update();
                _currentlyWorkingMowingStep = null;
            }

            return false;
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