using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using MowingMachine.Models;
using MowingMachine.Services;

namespace MowingMachine
{
    /// <summary>
    /// Interaction logic for SampleMapPage.xaml
    /// </summary>
    public partial class SampleMapPage : Page
    {
        private readonly MyMowingMachine mowingMachine;

        public SampleMapPage(int[][] map)
        {
            InitializeComponent();

            var mapManager = new MapManager(map);
            mowingMachine = new MyMowingMachine(mapManager);

            mapManager.OnUpdateMap += UpdateMap;
            
            var (columnDefinitions, rowDefinitions) =
                Common.GenerateDefinitions(map.Length, map.Length);

            CreateDefinitions(columnDefinitions, rowDefinitions);
            
            Render(map);
        }
        
        
        public void ExecuteStep()
        {
            var isComplete = mowingMachine.PerformMove();

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

        private void Render(IEnumerable<int[]> map)
        {
            var elements = Common.GetUiElements(map.Reverse().ToArray());
            
            SimulationGrid.Children.Clear();
            foreach (var uiElement in elements)
                SimulationGrid.Children.Add(uiElement);
        }
        
        private void UpdateMap(object _, MapManager.OnUpdateMapEventArgs args)
        {
            Render(args.Map);
        }
    }
}
