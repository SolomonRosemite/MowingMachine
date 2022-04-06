using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
        private readonly StringBuilder _movementStringBuilder = new();
        private string _movements = "";
        private int _movementCount;
        
        // Map and mowing machine configuration
        private double _mowingMachineCharge;
        private int[][] _newlyGeneratedMapSample;
        private int[][] _currentMapSample;
        private SampleMapPage _mapPage;
        private int _simulationSpeed;
        private int _simulationSize;

        // Used to control the UI state
        private bool _clearFirst;
        private bool _interrupted;
        private bool _running;
        
        public MainWindow()
        {
            InitializeComponent();

            ApplySettings(15, 20, 3, false);
        }

        private void InitializeApp()
        {
            var map = _newlyGeneratedMapSample ?? Constants.DefaultMapSample;

            if (map is null)
            {
                GenerateNewMap();
                map = _newlyGeneratedMapSample;
            }
            
            _currentMapSample = map;

            var mapSample = _currentMapSample.DeepClone();
            mapSample = mapSample.ToArray();

            _mapPage = new SampleMapPage(mapSample, _mowingMachineCharge, this);
            SampleMapFrame.Content = _mapPage;

            _running = false;
            _movements = string.Empty; 
            _movementCount = 0; 
            SetChargeValue(0);
            SetMowedGrassValue(0);

            _movementStringBuilder.Clear();
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
            Console.WriteLine($"Running with {_simulationSize}X{_simulationSize} map size.");

            UpdateUi();
            
            _interrupted = false;
            _clearFirst = true;
            var stopwatch = new Stopwatch();
            stopwatch.Start();
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
                    stopwatch.Stop();
                    var mowingElapsedTime = stopwatch.Elapsed.TotalSeconds;
                    var mowingElapsedTimeStr = $"Mowing complete and took {mowingElapsedTime} seconds!";

                    Console.WriteLine(mowingElapsedTimeStr);
                    AddMovement(mowingElapsedTimeStr);
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
            if (int.TryParse(SimulationSpeedTextBox.Text, out var simulationSpeed) && double.TryParse(BatteryCapacityTextBox.Text, out var batteryCapacity) && int.TryParse(SimulationSizeTextBox.Text, out var simulationSize))
            {
                ApplySettings(simulationSpeed, simulationSize, batteryCapacity);
                return;
            }
         
            ShowPopup("Invalid settings input", "The specified settings are not valid. Please try again.");
        }

        private void ApplySettings(int simulationSpeed, int simulationSize, double batteryCapacity, bool showDialog = true)
        {
            SimulationSizeTextBox.Text = simulationSize.ToString();
            SimulationSpeedTextBox.Text = simulationSpeed.ToString();
            BatteryCapacityTextBox.Text = batteryCapacity.ToString();

            _simulationSize = simulationSize;
            _simulationSpeed = simulationSpeed;
            _mowingMachineCharge = batteryCapacity * 1000;

            if (showDialog)
                ShowPopup("Updated setting", "Updated settings successfully.\nSettings will be applied on the next simulation.");
            InitializeApp();
        }

        private void OnApplySettingsButtonClick(object sender, RoutedEventArgs e) => ApplySettings();

        private void OnResetSettingsButtonClick(object sender, RoutedEventArgs e) => ApplySettings(15, 20, 3);
        private void OnSaveCurrentMapButtonClick(object sender, RoutedEventArgs e) => Constants.SaveMapAsJson(_currentMapSample);
        
        private void OnGenerateNewMapClick(object sender, RoutedEventArgs e)
        {
            _newlyGeneratedMapSample = _currentMapSample.DeepClone();

            GenerateNewMap();
            InitializeApp();
        }

        private void GenerateNewMap()
        {
            int size = _simulationSize;
            var rng = new Random();
            var map = MapGeneration.GenerateMapGetMap(size, rng.Next());
            
            map[rng.Next(size)][rng.Next(size)] = (int) FieldType.MowingMachine;
            
            _newlyGeneratedMapSample = map.DeepClone();
        }

        private readonly StringBuilder _movementStringBuilderForCurrent = new();
        
        private void AddMovement(string movement)
        {
            _movementStringBuilderForCurrent.Append(++_movementCount);
            _movementStringBuilderForCurrent.Append(": ");
            _movementStringBuilderForCurrent.Append(movement);
            
            _movementStringBuilder.AppendLine(_movementStringBuilderForCurrent.ToString());
            _movements = _movementStringBuilder.ToString();
            
            MovementsTextBox.Text = _movements;

            MovementsTextBox.ScrollToEnd();
            _movementStringBuilderForCurrent.Clear();
        }

        private void ShowPopup(string title, string description)
        {
            new Popup(title, description).Show();
        }
    }
}