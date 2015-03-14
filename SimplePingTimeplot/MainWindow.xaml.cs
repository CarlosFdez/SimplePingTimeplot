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

using System.Reactive;
using System.Reactive.Linq;
using System.Net.NetworkInformation;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Axes;
using System.ComponentModel;

namespace SimplePingTimeplot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
    }

    public class PingTracker
    {
        private class PingTimestamp
        {
            public DateTime Time { get; set; }
            public int Ping {get; set; }
        }

        private bool isRunning;
        private IDisposable unsubscriber;
        private List<PingTimestamp> pingTimestamps = new List<PingTimestamp>();

        public IEnumerable<DataPoint> PingPoints { get; private set; }

        public string Label { get; private set; }
        public string Website { get; private set; }

        public delegate void OnUpdate();

        public PingTracker(string label, string website)
        {
            Label = label;
            Website = website;
            PingPoints = pingTimestamps.Select(t => new DataPoint((DateTime.Now - t.Time).TotalMinutes, t.Ping));
        }

        public void Start(OnUpdate onUpdate)
        {
            if (isRunning)
            {
                throw new InvalidOperationException("already started");
            }

            PerformUpdate(onUpdate);
            unsubscriber = Observable
                .Interval(TimeSpan.FromSeconds(5))
                .Subscribe(idx => PerformUpdate(onUpdate));

            isRunning = true;
        }

        private void PerformUpdate(OnUpdate onUpdate)
        {
            var timestamp = DateTime.Now;
            var ping = GetPing();

            pingTimestamps.Add(new PingTimestamp() { Time = timestamp, Ping = (int)ping });

            onUpdate();
        }

        public void Stop()
        {
            if (!isRunning)
            {
                throw new InvalidOperationException("not running");
            }

            unsubscriber.Dispose(); // top the asynchronous
            isRunning = false;
        }

        private long GetPing()
        {
            return new Ping().Send(Website).RoundtripTime;
        }
    }

    public class ApplicationViewModel : INotifyPropertyChanged
    {
        public PlotModel PlotModel { get; set; }

        public ApplicationViewModel()
        {
            PlotModel = new PlotModel();

            PlotModel.Axes.Add(new LinearAxis()
                {
                    Position = AxisPosition.Bottom,
                    Title = "Time Ago",
                    Unit = "Minutes",
                    EndPosition = 0,
                    StartPosition = 1,
                    IsZoomEnabled = false
                });

            PlotModel.Axes.Add(new LinearAxis()
                {
                    Position = AxisPosition.Left,
                    Title = "Ping",
                    Unit = "Milliseconds",
                    Minimum = 0,
                    Maximum = 700,
                    IsZoomEnabled = false
                });

            AddSite("Google", "www.google.com");
        }

        public void AddSite(string label, string website)
        {
            // todo: allow us a way to remove it and unsubscribe
            var series = new LineSeries();
            PlotModel.Series.Add(series);
            series.Title = label;

            var tracker = new PingTracker(label, website);
            series.ItemsSource = tracker.PingPoints;
            tracker.Start(() => PlotModel.InvalidatePlot(true));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
    }
}
