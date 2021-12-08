using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Windows;
using MowingMachine.Models;

namespace MowingMachine
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly double _mowingMachineCharge = 1200;
        private int[][] _initialMapSample;
        private SampleMapPage _mapPage;
        private Timer _timer;
        private bool _running;
        
        public MainWindow()
        {
            InitializeComponent();

            InitializeApp();
        }

        private void InitializeApp()
        {
            _timer?.Stop();
            
            int[][] mapSample =
            {
                new [] { 1, 1, 1, 1, 1, 1, 1, 6, 6, 6 },
                new [] { 1, 1, 6, 6, 6, 1, 1, 1, 1, 6 },
                new [] { 1, 1, 6, 6, 6, 6, 1, 1, 1, 6 },
                new [] { 1, 1, 6, 6, 6, 6, 1, 1, 1, 1 },
                new [] { 1, 1, 1, 1, 1, 3, 1, 1, 1, 1 },
                new [] { 1, 1, 1, 1, 1, 3, 1, 1, 1, 1 },
                new [] { 1, 1, 1, 1, 1, 3, 1, 1, 5, 1 },
                new [] { 1, 6, 3, 3, 3, 3, 1, 0, 0, 0 },
                new [] { 1, 1, 3, 1, 1, 1, 1, 0, 6, 0 },
                new [] { 1, 1, 3, 1, 1, 1, 0, 0, 0, 0 },
            };

            _initialMapSample = mapSample.Select(a => a.ToArray()).ToArray();;
            mapSample = mapSample.Reverse().ToArray();

            _mapPage = new SampleMapPage(mapSample, _mowingMachineCharge, this);
            SampleMapFrame.Content = _mapPage;
        }

        public void UpdateValues(MapManager.OnUpdateMapEventArgs e)
        {
            var totalGrass = GetCount(_initialMapSample, FieldType.Grass) - 1;
            var totalMowedGrass = GetCount((int[][]) e.Map.Clone(), FieldType.MowedLawn);
            
            Application.Current.Dispatcher.Invoke(delegate
            {
                // Update charge
                ChargeProgressBar.Value = Math.Round(e.Charge / _mowingMachineCharge * 100, 2);
                ChargeLabel.Content = $"Charge: {ChargeProgressBar.Value}%";
                
                var newValue = totalMowedGrass / totalGrass * 100;
                MowedGrassCountProgressBar.Value = Math.Round(Math.Max(newValue, MowedGrassCountProgressBar.Value), 2);
                MowedGrassCountLabel.Content = $"Mowed lawn: {MowedGrassCountProgressBar.Value}%";
            });
        }

        private double GetCount(IReadOnlyList<int[]> map, FieldType type)
        {
            double count = 0;
            
            for (int x = 0; x < map.Count; x++)
                for (int y = 0; y < map.Count; y++)
                    if (map[x][y] == (int) type)
                        count++;
            
            return count;
            // return map.Sum(t1 => map.Where((t, y) => (FieldType) t1[y] == type).Count());
        }

        private void StartSimulationClick(object sender, RoutedEventArgs e)
        {
            _running = !_running;
            
            UpdateUi();
            
            if (_running)
                RunSimulation();
            else
                InitializeApp();
        }

        private void UpdateUi() => StartButton.Content = _running ? "Stop simulation" : "Start again simulation";

        private void RunSimulation()
        {
            _timer = new Timer(10);
            _timer.AutoReset = true;
            _timer.Elapsed += TimerOnElapsed;
            _timer.Start();
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            var complete = _mapPage.ExecuteStep();
            if (complete)
                _timer.Stop();
        }
    }
}