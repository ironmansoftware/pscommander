using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Wpf;
 
namespace pscommander
{
    /// <summary>
    /// Interaction logic for MeasurementCard.xaml
    /// </summary>
    public partial class MeasurementCard : UserControl, INotifyPropertyChanged
    {
        private double _lastLecture;
        private double _trend;
        private MeasurementDesktopWidget _widget;
        private CancellationTokenSource _source;
        private MeasurementTheme _theme;

        public void Cancel()
        {
            _source.Cancel();
        }
 
        public MeasurementCard(MeasurementDesktopWidget widget, PowerShellService powerShellService)
        {
            _widget = widget;

            _theme = pscommander.MeasurementTheme.Themes[_widget.Theme];
            _source = new CancellationTokenSource();
            InitializeComponent();
 
            LastHourSeries = new SeriesCollection
            {
                new LineSeries
                {
                    Values = new ChartValues<ObservableValue>()
                }
            };
            Task.Run(() =>
            {
                while (true)
                {
                    try 
                    {
                        _trend = Convert.ToDouble(powerShellService.Execute<object>(_widget.LoadMeasurement).First());
                    }
                    catch {}

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        LastHourSeries[0].Values.Add(new ObservableValue(_trend));
                        if (LastHourSeries[0].Values.Count > _widget.History)
                            LastHourSeries[0].Values.RemoveAt(0);
                        LastLecture = _trend;
                    });

                    Thread.Sleep(_widget.Frequency * 1000);
                    if (_source.IsCancellationRequested) break;
                }
            }, _source.Token);
 
            DataContext = this;
        }

        public string MeasurementTitle => _widget.Title;
        public string MeasurementSubTitle => _widget.Subtitle;
        public string MeasurementDescription => _widget.Description;
        public string MeasurementUnit  => _widget.Unit;
        public string Stroke => _theme.Stroke;
        public string Fill => _theme.Fill;
        public string TextBackground => _theme.TextBackground;
        public string ChartBackground => _theme.ChartBackground;
        public string Subtitle => _theme.Subtitle;
        public string Title => _theme.Title;
        public string TextForeground => _theme.TextForeground;


        public SeriesCollection LastHourSeries { get; set; }
        
        public double LastLecture
        {
            get { return _lastLecture; }
            set
            {
                _lastLecture = value;
                OnPropertyChanged("LastLecture");
            }
        }
 
        public event PropertyChangedEventHandler PropertyChanged;
 
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}