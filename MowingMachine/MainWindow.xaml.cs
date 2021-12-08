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
        private SampleMapPage _mapPage;
        private Timer _timer;
        private bool _running;
        private int[][] _initialMapSample;
        
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

            _initialMapSample = (int[][]) mapSample.Clone();
            mapSample = mapSample.Reverse().ToArray();

            _mapPage = new SampleMapPage(mapSample, 1200);
            SampleMapFrame.Content = _mapPage;
            
            // _mapPage.OnUpdateMap += Ok;
        }

        private void Ok(object sender, MapManager.OnUpdateMapEventArgs e)
        {
            var totalGrass = GetCount(_initialMapSample, FieldType.Grass);
            var totalMowedGrass = GetCount(e.Map, FieldType.MowedLawn);
            
            // MowedGrassCountProgressBar.Value = totalMowedGrass / totalGrass;
            // ChargeProgressBar.Value = e.Charge;
        }

        private double GetCount(int[][] map, FieldType type)
        {
            return map.Sum(t1 => map.Where((t, y) => (FieldType) t1[y] == type).Count());
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