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
                    Unit = "Milliseconds"
                });

            PlotModel.Axes.Add(new LinearAxis()
                {
                    Position = AxisPosition.Left,
                    Title = "Ping",
                    Unit = "Milliseconds",
                    AbsoluteMinimum = 0,
                    AbsoluteMaximum = 2000,
                    Minimum = 0,
                    Maximum = 700
                });

            AddSite("Google", "www.google.com");
        }

        public void AddSite(string label, string website)
        {
            // todo: allow us a way to remove it and unsubscribe
            var series = new LineSeries();
            PlotModel.Series.Add(series);
            series.Title = label;

            Observable.Interval(TimeSpan.FromSeconds(1))
                .Subscribe((idx) =>
                {
                    var timestamp = DateTime.Now;
                    var ping = GetPing();

                    Console.WriteLine(PlotModel.Series.Count());
                    series.Points.Add(new DataPoint(idx, ping));
                    PlotModel.InvalidatePlot(true);
                });

        }

        public long GetPing()
        {
            return new Ping().Send("www.google.com").RoundtripTime;
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
