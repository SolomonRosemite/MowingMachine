using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MowingMachine.Common;
using MowingMachine.Models;

namespace MowingMachine
{
    /// <summary>
    /// Interaction logic for SampleMapPage.xaml
    /// </summary>
    public partial class SampleMapPage : Page
    {
        private readonly MyMowingMachine _mowingMachine;
        private int[][] _prevMapState;
        
        public SampleMapPage(int[][] map, double mowingMachineEnergy, MainWindow mainWindow)
        {
            InitializeComponent();

            var mapManager = new MapManager(map, mowingMachineEnergy, mainWindow);
            _mowingMachine = new MyMowingMachine(mainWindow, mapManager, mowingMachineEnergy);

            mapManager.OnUpdateMap += UpdateMap;
            
            var (columnDefinitions, rowDefinitions) =
                Common.Common.GenerateDefinitions(map.Length, map.Length);

            CreateDefinitions(columnDefinitions, rowDefinitions);
            
            Render(map);
        }
        
        public bool ExecuteStep()
        {
            return _mowingMachine.PerformMove();
        }
        
        private void CreateDefinitions(IReadOnlyList<ColumnDefinition> columnDefinitions, IReadOnlyList<RowDefinition> rowDefinitions)
        {
            for (int i = 0; i < rowDefinitions.Count; i++)
                SimulationGrid.RowDefinitions.Add(rowDefinitions[i]);

            for (int i = 0; i < columnDefinitions.Count; i++)
                SimulationGrid.ColumnDefinitions.Add(columnDefinitions[i]);
        }

        private void Render(int[][] map)
        {
            map = map.Reverse().ToArray();
            Application.Current.Dispatcher.Invoke(delegate{
                if (_prevMapState is null)
                {
                    var elements = Common.Common.GetUiElements(map);
                    
                    SimulationGrid.Children.Clear();
                    foreach (var uiElement in elements)
                        SimulationGrid.Children.Add(uiElement);
                }
                else
                {
                    bool foundChangedField = false;
                    int foundFieldIndex1 = 0, foundFieldIndex2 = 0, fieldType1 = 0, fieldType2 = 0;
                    
                    for (int x = 0; x < map.Length; x++)
                        for (int y = 0; y < map.Length; y++)
                            if (map[y][x] != _prevMapState[y][x])
                            {
                                OnFoundChangedField(map,
                                    x,
                                    y,
                                    out foundFieldIndex1,
                                    out foundFieldIndex2,
                                    out fieldType1,
                                    out fieldType2);
                                foundChangedField = true;
                                break;
                            }

                    if (foundChangedField is false)
                        return;

                    SimulationGrid.Children.RemoveAt(foundFieldIndex1);
                    SimulationGrid.Children.Insert(foundFieldIndex1,
                        Common.Common.GetUiElement(fieldType1, foundFieldIndex1 % map.Length, foundFieldIndex1 / map.Length));

                    SimulationGrid.Children.RemoveAt(foundFieldIndex2);
                    SimulationGrid.Children.Insert(foundFieldIndex2,
                        Common.Common.GetUiElement(fieldType2, foundFieldIndex2 % map.Length, foundFieldIndex2 / map.Length));
                }
                
                _prevMapState = map.DeepClone();
            });
        }

        private void UpdateMap(object _, MapManager.OnUpdateMapEventArgs args)
        {
            Render(args.Map);
        }

        private void OnFoundChangedField(
            int[][] map,
            int x,
            int y,
            out int foundFieldIndex1,
            out int foundFieldIndex2,
            out int fieldType1,
            out int fieldType2)
        {
            (fieldType1, foundFieldIndex1) = FieldChangedOrDefault(map, _prevMapState, x, y)!.Value;

            var results = new []
            {
                FieldChangedOrDefault(map, _prevMapState, x + 1, y),
                FieldChangedOrDefault(map, _prevMapState, x - 1, y),
                FieldChangedOrDefault(map, _prevMapState, x, y + 1),
                FieldChangedOrDefault(map, _prevMapState, x, y - 1),
            };

            (fieldType2, foundFieldIndex2) = results.Single(r => r != null).Value;
        }

        private static (int, int)? FieldChangedOrDefault(int[][] map, int[][] previousMap, int x, int y)
        {
            if (IsOutOfBounds(map.Length, y)) return null;
            if (IsOutOfBounds(map[y].Length, x)) return null;

            if (map[y][x] == previousMap[y][x]) return null;
            
            return (map[y][x], TranslateTwoDimensionalPointToOneDimension(map.Length, x, y));
        }

        private static int TranslateTwoDimensionalPointToOneDimension(int height, int x, int y)
        {
            return height * y + x;
        }

        private static bool IsOutOfBounds(int arrLength, int index) => index >= arrLength || index < 0;
    }
}
