using System.Linq;
using System.Timers;
using System.Windows;

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

            mapSample = mapSample.Reverse().ToArray();

            _mapPage = new SampleMapPage(mapSample, 1200);
            SampleMapFrame.Content = _mapPage;
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