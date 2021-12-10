using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using MowingMachine.Common;
using MowingMachine.Models;

namespace MowingMachine
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Used to show the mowing moves
        private string _movements = "";
        private int _movementCount;
        
        // map sample and mowing machine settings 
        private double _mowingMachineCharge;
        private int[][] _newlyGeneratedMapSample;
        private int[][] _currentMapSample;
        private SampleMapPage _mapPage;
        private int _simulationSpeed;

        // Used to control the UI state
        private bool _clearFirst;
        private bool _interrupted;
        private bool _running;
        
        public MainWindow()
        {
            InitializeComponent();

            ApplySettings(10, 1.2, false);
        }

        private void InitializeApp()
        {
            _currentMapSample = _newlyGeneratedMapSample ?? Constants.DefaultMapSample;

            var mapSample = _currentMapSample.DeepClone();
            mapSample = mapSample.Reverse().ToArray();

            _mapPage = new SampleMapPage(mapSample, _mowingMachineCharge, this);
            SampleMapFrame.Content = _mapPage;

            _running = false;
            _movements = string.Empty; 
            _movementCount = 0; 
            SetChargeValue(0);
            SetMowedGrassValue(0);
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

        private async void StartSimulationClick(object sender, RoutedEventArgs e)
        {
            if (_clearFirst)
            {
                _interrupted = true;
                _clearFirst = false;
                UpdateUi();
                
                InitializeApp();
            } else if (_running)
            {
                _running = false;
                
                // If we are already running reset app.
                InitializeApp();
            }
            else
            {
                _running = true;
                await RunSimulation();
            }
        }

        private void UpdateUi() => StartButton.Content = _clearFirst ? "Clear map" : _running ? "Stop simulation" : "Start simulation again";

        private async Task RunSimulation()
        {
            Console.WriteLine($"Running with {_mowingMachineCharge} charge.");
            Console.WriteLine($"Running with {_simulationSpeed} ms per step. (Speed)");

            UpdateUi();
            
            _interrupted = false;
            _clearFirst = true;
            while (true)
            {
                if (_interrupted && _running)
                {
                    _interrupted = false;
                    _running = false;
                    UpdateUi();
                    break;
                }
                
                if (!_running)
                {
                    AddMovement("Mowing complete!");
                    UpdateUi();
                    break;
                }
                
                await Task.Delay(_simulationSpeed);
                ExecuteNextStep();
            }
        }

        private void ExecuteNextStep()
        {
            var complete = _mapPage.ExecuteStep();
            _running = !complete;
        }
        
        public void UpdateValues(MapManager.OnUpdateMapEventArgs e)
        {
            var totalGrass = GetCount(_currentMapSample, FieldType.Grass) - 1;
            var totalMowedGrass = GetCount((int[][]) e.Map.Clone(), FieldType.MowedLawn);

            Application.Current.Dispatcher.Invoke(delegate
            {
                // Update charge
                SetChargeValue(e.Charge);

                var newValue = totalMowedGrass / totalGrass * 100;
                SetMowedGrassValue(Math.Max(newValue, MowedGrassCountProgressBar.Value));
                
                AddMovement(e.Movement);
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

        private void ApplySettings()
        {
            if (int.TryParse(SimulationSpeedTextBox.Text, out var simulationSpeed) && double.TryParse(BatteryCapacityTextBox.Text, out var batteryCapacity))
            {
                ApplySettings(simulationSpeed, batteryCapacity);
                return;
            }
         
            ShowPopup("Invalid settings input", "The specified settings are not valid. Please try again.");
        }

        private void ApplySettings(int simulationSpeed, double batteryCapacity, bool showDialog = true)
        {
            SimulationSpeedTextBox.Text = simulationSpeed.ToString();
            BatteryCapacityTextBox.Text = batteryCapacity.ToString();
            
            _simulationSpeed = simulationSpeed;
            _mowingMachineCharge = batteryCapacity * 1000;

            if (showDialog)
                ShowPopup("Updated setting", "Updated settings successfully.\nSettings will be applied on the next simulation.");
            InitializeApp();
        }

        private void OnApplySettingsButtonClick(object sender, RoutedEventArgs e) => ApplySettings();

        private void OnResetSettingsButtonClick(object sender, RoutedEventArgs e) => ApplySettings(10, 1.2);

        private void OnGenerateNewMapClick(object sender, RoutedEventArgs e)
        {
            _newlyGeneratedMapSample = _currentMapSample.DeepClone();

            GenerateNewMap();
            InitializeApp();
        }

        private void GenerateNewMap()
        {
            var rng = new Random();
            
            // Configuration
            const int mapSize = 10;
            var map = new int[mapSize][];
            
            var chanceOfFieldAsList = new Dictionary<FieldType, int>
            {
                { FieldType.Grass, 60 },
                { FieldType.Sand, 15 },
                { FieldType.CobbleStone, 15 },
                { FieldType.Water, 10 },
            }.ToList();
            
            for (int x = 0; x < map.Length; x++)
            {
                map[x] = new int[mapSize];
                
                for (int y = 0; y < map.Length; y++)
                    map[x][y] = (int) GetRandom(chanceOfFieldAsList);
            }

            map[5][7] = (int) FieldType.MowingMachine;
            
            _newlyGeneratedMapSample = map.DeepClone();
            
            FieldType GetRandom(IReadOnlyList<KeyValuePair<FieldType, int>> items)
            {
                int index = rng.Next(items.Select(i => i.Value).Sum());

                int sum = 0;
                int i = 0;
                while (sum < index)
                    sum += items[i++].Value;

                return items[Math.Max(0, i - 1)].Key;
            }
        }

        private void AddMovement(string movement)
        {
            _movements += $"{++_movementCount}: {movement}\n";
            MovementsTextBox.Text = _movements;
            MovementsTextBox.ScrollToEnd();
        }

        private void ShowPopup(string title, string description)
        {
            new Popup(title, description).Show();
        }
    }
}