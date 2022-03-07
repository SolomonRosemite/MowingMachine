using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MowingMachine.Models;

namespace MowingMachine
{
    /// <summary>
    /// Interaction logic for SampleMapPage.xaml
    /// </summary>
    public partial class SampleMapPage : Page
    {
        private readonly MyMowingMachine _mowingMachine;
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

        private void Render(IEnumerable<int[]> map)
        {          
            Application.Current.Dispatcher.Invoke(delegate{
                var elements = Common.Common.GetUiElements(map.Reverse().ToArray());
                
                SimulationGrid.Children.Clear();
                foreach (var uiElement in elements)
                    SimulationGrid.Children.Add(uiElement);
            });
        }

        private void UpdateMap(object _, MapManager.OnUpdateMapEventArgs args)
        {
            Render(args.Map);
        }
    }
}
