using System;
using System.Collections.Generic;
using System.Runtime.Intrinsics.X86;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using MowingMachine.Services;

namespace MowingMachine
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Console.WriteLine("Test");
            
            InitialRun();
        }

        private void InitialRun()
        {
            var (columnDefinitions, rowDefinitions) =
                // MowingMachineService.GenerateDefinitions(3, 3);
                MowingMachineService.GenerateDefinitions(10, 10);

            ProcessData(columnDefinitions, rowDefinitions);
        }

        private void ProcessData(ColumnDefinition[] columnDefinitions, RowDefinition[] rowDefinitions)
        {
            for (int i = 0; i < rowDefinitions.Length; i++)
                SimulationGrid.RowDefinitions.Add(rowDefinitions[i]);

            for (int i = 0; i < columnDefinitions.Length; i++)
                SimulationGrid.ColumnDefinitions.Add(columnDefinitions[i]);

            var elements = MowingMachineService.GetUiElements(columnDefinitions, rowDefinitions);
            foreach (var uiElement in elements)
                SimulationGrid.Children.Add(uiElement);
        }
    }
}