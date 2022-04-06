using System;
using MowingMachine.Common;

namespace MowingMachine.Models
{
    public class MapManager
    {
        public class OnUpdateMapEventArgs : EventArgs
        {
            public int[][] Map;
            public double Charge;
            public string Movement;
        }
        
        public MapManager(SampleMapPage sampleMapPage, int[][] map, double fuel, MainWindow mainWindow)
        {
            this._map = map;
            _currentFuel = fuel;
            _mainWindow = mainWindow;
            _sampleMapPage = sampleMapPage;
        }

        private MowingStep _currentlyWorkingMowingStep;
        private readonly MainWindow _mainWindow;
        private readonly SampleMapPage _sampleMapPage;
        private readonly int[][] _map;
        private double _currentFuel;

        public FieldType MoveMowingMachine(MowingStep step, FieldType previousField, double fuel)
        {
            _currentlyWorkingMowingStep = step;
            _currentFuel = fuel;
            
            var (mowingMachineX, mowingMachineY) = GetMowingMachineCoordinate();
            var (x, y) = _map.GetTranslatedCoordinate(mowingMachineX, mowingMachineY, step.MoveDirection);
            var nextPreviousField = (FieldType) _map[y][x];
            
            _map[mowingMachineY][mowingMachineX] = (int) previousField;
            _map[y][x] = (int) FieldType.MowingMachine;

            return nextPreviousField;
        }

        private void Update(string movement)
        {
            var args = new OnUpdateMapEventArgs {Map = (int[][]) this._map.Clone(), Charge = _currentFuel, Movement = movement};
            _mainWindow.UpdateValues(args);
            _sampleMapPage.UpdateMap(args);
        }

        public bool Verify()
        {
            if (_currentlyWorkingMowingStep is null)
                return true;
            
            if (_currentlyWorkingMowingStep.Turns.Count != 0)
            {
                var moveDirection = _currentlyWorkingMowingStep.Turns.Dequeue();
                Update($"Turned mowing machine in direction: {moveDirection}.");
            }
            else
            {
                Update("Mowing machine moving forward.");
                _currentlyWorkingMowingStep = null;
            }

            return false;
        }

        public FieldOfView GetFieldsOfView()
        {
            var (x, y) = GetMowingMachineCoordinate();
            return new FieldOfView(GetFieldsAroundCoordinate(x, y));
        }
        
        private int[][] GetFieldsAroundCoordinate(int xCoord, int yCoord)
        {
            var map2 = new[]
            {
                new[]
                {
                    _map.GetFieldInverted(xCoord - 1, yCoord - 1),
                    _map.GetFieldInverted(xCoord, yCoord - 1),
                    _map.GetFieldInverted(xCoord + 1, yCoord - 1),
                },
                new[]
                {
                    _map.GetFieldInverted(xCoord - 1, yCoord),
                    _map.GetFieldInverted(xCoord, yCoord),
                    _map.GetFieldInverted(xCoord + 1, yCoord),
                },
                new[]
                {
                    _map.GetFieldInverted(xCoord - 1, yCoord - 1),
                    _map.GetFieldInverted(xCoord, yCoord + 1),
                    _map.GetFieldInverted(xCoord + 1, yCoord + 1),
                },
            };

            return map2;
        }
        
        private (int, int) GetMowingMachineCoordinate()
        {
            for (int y = 0; y < _map.Length; y++)
            {
                for (int x = 0; x < _map.Length; x++)
                {
                    FieldType type = (FieldType)_map[y][x];

                    if (type == FieldType.MowingMachine)
                        return (x, y);
                }
            }

            throw new Exception("Could not find Mowing Machine.");
        }
    }
}