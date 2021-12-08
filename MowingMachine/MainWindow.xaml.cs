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
        private double _mowingMachineCharge;
        private int[][] _initialMapSample;
        private SampleMapPage _mapPage;
        private Timer _timer;
        private bool _running;
        private int _simulationSpeed;
        
        public MainWindow()
        {
            InitializeComponent();

            InitializeApp();
        }

        // Todo: Improve UI so that when the values change we make sure to call the Initialize method again. Else we dont actually use the user input.
        // It gets applied once the sim has ended and then the user clicks "start sim again" which will cause the Initialize method to be called again.
        private void InitializeApp()
        {
            _timer?.Stop();

            Console.WriteLine(SimulationSpeedTextBox.Text);
            Console.WriteLine(BatteryCapacityTextBox.Text);
            
            if (int.TryParse(SimulationSpeedTextBox.Text, out var simulationSpeed) && double.TryParse(BatteryCapacityTextBox.Text, out var batteryCapacity))
            {
                _simulationSpeed = simulationSpeed;
                _mowingMachineCharge = batteryCapacity * 100;
            }
            else
            {
                // Todo: User typed invalid input. Handle here...
                return;
            }
            
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
            
            SetChargeValue(0);
            SetMowedGrassValue(0);
        }

        public void UpdateValues(MapManager.OnUpdateMapEventArgs e)
        {
            var totalGrass = GetCount(_initialMapSample, FieldType.Grass) - 1;
            var totalMowedGrass = GetCount((int[][]) e.Map.Clone(), FieldType.MowedLawn);
            
            Application.Current.Dispatcher.Invoke(delegate
            {
                // Update charge
                SetChargeValue(e.Charge);
                
                var newValue = totalMowedGrass / totalGrass * 100;
                SetMowedGrassValue(Math.Max(newValue, MowedGrassCountProgressBar.Value));
            });
        }

        private void SetMowedGrassValue(double value)
        {
            MowedGrassCountProgressBar.Value = Math.Round(value, 2);
            MowedGrassCountLabel.Content = $"Mowed lawn: {MowedGrassCountProgressBar.Value}%";
        }

        private void SetChargeValue(double value)
        {
            ChargeProgressBar.Value = Math.Round(value / _mowingMachineCharge * 100, 2);
            ChargeLabel.Content = $"Charge: {ChargeProgressBar.Value}%";
        }

        private double GetCount(IReadOnlyList<int[]> map, FieldType type)
        {
            double count = 0;
            
            for (int x = 0; x < map.Count; x++)
                for (int y = 0; y < map.Count; y++)
                    if (map[x][y] == (int) type)
                        count++;
            
            return count;
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

        private void UpdateUi() => StartButton.Content = _running ? "Stop simulation" : "Start simulation again";

        private void RunSimulation()
        {
            _timer = new Timer(_simulationSpeed);
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