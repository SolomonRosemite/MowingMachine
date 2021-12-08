using System;
using MowingMachine.Common;

namespace MowingMachine.Models
{
    public class MapManager
    {
        public event EventHandler<OnUpdateMapEventArgs> OnUpdateMap;
        
        public class OnUpdateMapEventArgs : EventArgs
        {
            public int[][] Map;
            public double Charge;
        }
        
        public MapManager(int[][] map, double fuel)
        {
            this.Map = map;
            _currentFuel = fuel;
        }

        private MowingStep _currentlyWorkingMowingStep;
        private double _currentFuel;
        private int[][] Map { get; }
        
        public FieldType MoveMowingMachine(MowingStep step, FieldType previousField, double fuel)
        {
            _currentlyWorkingMowingStep = step;
            _currentFuel = fuel;
            
            var (mowingMachineX, mowingMachineY) = GetMowingMachineCoordinate();
            var (x, y) = Map.GetTranslatedCoordinate(mowingMachineX, mowingMachineY, step.MoveDirection);

            var nextPreviousField = (FieldType) Map[x][y];
            
            Map[mowingMachineX][mowingMachineY] = (int) previousField;
            Map[x][y] = (int) FieldType.MowingMachine;

            return nextPreviousField;
        }

        private void Update()
        {
            OnUpdateMap?.Invoke(this, new OnUpdateMapEventArgs { Map = (int[][]) this.Map.Clone(), Charge = _currentFuel});
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
                Console.WriteLine("Mowing machine moving forward.");
                Update();
                _currentlyWorkingMowingStep = null;
            }

            return false;
        }

        public FieldOfView GetFieldsOfView()
        {
            var (x, y) = GetMowingMachineCoordinate();
            return new FieldOfView(GetFieldsAroundCoordinate(x, y));
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