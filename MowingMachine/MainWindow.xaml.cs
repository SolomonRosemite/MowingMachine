using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using MowingMachine.Models;
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

            int[][] sample = 
            {
                new [] { 1, 1, 1, 1, 1, 1, 1, 6, 6, 6 },
                new [] { 1, 1, 6, 6, 6, 1, 1, 1, 1, 6 },
                new [] { 1, 1, 6, 6, 6, 6, 1, 1, 1, 6 },
                new [] { 1, 1, 6, 6, 6, 6, 1, 1, 1, 1 },
                new [] { 1, 1, 1, 1, 1, 3, 1, 1, 1, 1 },
                new [] { 1, 1, 1, 1, 1, 3, 1, 1, 1, 1 },
                new [] { 1, 1, 1, 1, 1, 3, 1, 1, 5, 4 },
                new [] { 1, 6, 3, 3, 3, 3, 1, 0, 0, 0 },
                new [] { 1, 1, 3, 1, 1, 1, 1, 0, 6, 0 },
                new [] { 1, 1, 3, 1, 1, 1, 0, 0, 0, 0 },
            };

            sample = sample.Reverse().ToArray();
            
            SampleMapFrame.Content = new SampleMapPage(sample);
        }

        private void StartSimulationClick(object sender, RoutedEventArgs e)
        {
            (SampleMapFrame.Content as SampleMapPage)?.DoStep();
        }
    }
}