using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using MowingMachine.Services;

namespace MowingMachine
{
    /// <summary>
    /// Interaction logic for SampleMapPage.xaml
    /// </summary>
    public partial class SampleMapPage : Page
    {
        public SampleMapPage(int[][] sample)
        {
            InitializeComponent();
            
            var (columnDefinitions, rowDefinitions) =
                MowingMachineService.GenerateDefinitions(10, 10);

            ProcessData(columnDefinitions, rowDefinitions, sample);
        }
        
        private void ProcessData(IReadOnlyList<ColumnDefinition> columnDefinitions, IReadOnlyList<RowDefinition> rowDefinitions, int[][] sample)
        {
            for (int i = 0; i < rowDefinitions.Count; i++)
                SimulationGrid.RowDefinitions.Add(rowDefinitions[i]);

            for (int i = 0; i < columnDefinitions.Count; i++)
                SimulationGrid.ColumnDefinitions.Add(columnDefinitions[i]);

            var elements = MowingMachineService.GetUiElements(sample);
            foreach (var uiElement in elements)
                SimulationGrid.Children.Add(uiElement);
        }
    }
}
