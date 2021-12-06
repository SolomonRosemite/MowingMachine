using System;
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
        private SampleMapPage mapPage;
        private Timer timer;
        private bool running;
        
        public MainWindow()
        {
            InitializeComponent();

            InitializeApp();
        }

        private void InitializeApp()
        {
            timer?.Stop();
            
            int[][] sample = 
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

            sample = sample.Reverse().ToArray();

            mapPage = new SampleMapPage(sample);
            SampleMapFrame.Content = mapPage;
        }

        private void StartSimulationClick(object sender, RoutedEventArgs e)
        {
            running = !running;
            
            UpdateUi();
            
            if (running)
                RunSimulation();
            else
                InitializeApp();
        }

        private void UpdateUi()
        {
            StartButton.Content = running ? "Stop simulation" : "Start again simulation";
        }

        private void RunSimulation()
        {
            timer = new Timer(100);
            timer.AutoReset = true;
            timer.Elapsed += TimerOnElapsed;
            timer.Start();
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            var complete = mapPage.ExecuteStep();

            if (complete)
            {
                Application.Current.Dispatcher.Invoke(delegate {
                    StartSimulationClick(null, null);
                    running = true;
                });
            }
        }
    }
}