using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MowingMachine.Models;
using MowingMachine.Services;

namespace MowingMachine
{
    /// <summary>
    /// Interaction logic for SampleMapPage.xaml
    /// </summary>
    public partial class SampleMapPage : Page
    {
        private readonly MyMowingMachine _mowingMachine;
        
        public SampleMapPage(int[][] sample)
        {
            InitializeComponent();

            var mapManager = new MapManager(sample);
            _mowingMachine = new MyMowingMachine(mapManager.GetAllReachableCoordinates(), mapManager);

            mapManager.OnUpdateMap += UpdateMap;
            
            var (columnDefinitions, rowDefinitions) =
                MowingMachineService.GenerateDefinitions(sample.Length, sample.Length);

            CreateDefinitions(columnDefinitions, rowDefinitions);
            
            Render(sample);
        }
        
        
        public void ExecuteStep()
        {
            var isComplete = _mowingMachine.MakeMove();

            if (isComplete)
            {
                // Todo: Handle...
            }
        }
        
        private void CreateDefinitions(IReadOnlyList<ColumnDefinition> columnDefinitions, IReadOnlyList<RowDefinition> rowDefinitions)
        {
            for (int i = 0; i < rowDefinitions.Count; i++)
                SimulationGrid.RowDefinitions.Add(rowDefinitions[i]);

            for (int i = 0; i < columnDefinitions.Count; i++)
                SimulationGrid.ColumnDefinitions.Add(columnDefinitions[i]);
        }

        private void UpdateMap(object _, MapManager.OnUpdateMapEventArgs args)
        {
            Render(args.Map);
        }

        private void Render(IEnumerable<int[]> map)
        {
            var elements = MowingMachineService.GetUiElements(map.Reverse().ToArray());
            
            SimulationGrid.Children.Clear();
            foreach (var uiElement in elements)
                SimulationGrid.Children.Add(uiElement);
        }
    }
}
