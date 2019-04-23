using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using LiveCharts;
using LiveCharts.Configurations;

namespace Wpf.CartesianChart.ConstantChanges
{
    public partial class ConstantChangesChart : UserControl, INotifyPropertyChanged
    {
        private double _axisxMax;
        private double _axisxMin;
        private double _axisy0Max;
        private double _axisy0Min;
        private double _axisy1Max;
        private double _axisy1Min;
        private string _axisy0Title;
        private string _axisy1Title;
        private double _axisy0Step;
        private double _axisy1Step;

        private int MaxPoints = 150;

        // TODO: this graph should scale based on the amount of data received (up to ~12hrs)


        public ConstantChangesChart()
        {
            InitializeComponent();

            //To handle live data easily, in this case we built a specialized type
            //the MeasureModel class, it only contains 2 properties
            //DateTime and Value
            //We need to configure LiveCharts to handle MeasureModel class
            //The next code configures MeasureModel  globally, this means
            //that LiveCharts learns to plot MeasureModel and will use this config every time
            //a IChartValues instance uses this type.
            //this code ideally should only run once
            //you can configure series in many ways, learn more at 
            //http://lvcharts.net/App/examples/v1/wpf/Types%20and%20Configuration

            var mapper = Mappers.Xy<MeasureModel>()
                .X(model => model.DateTime.Ticks)   //use DateTime.Ticks as X
                .Y(model => model.Value);           //use the value property as Y

            //lets save the mapper globally.
            Charting.For<MeasureModel>(mapper);

            

            //the values property will store our values array
            ChartValuesA = new ChartValues<MeasureModel>();
            ChartValuesB = new ChartValues<MeasureModel>();


            //lets set how to display the X Labels
            DateTimeFormatter = value => new DateTime((long)value).ToString("hh:mm");

            //AxisStep forces the distance between each separator in the X axis
            AxisStep = TimeSpan.FromHours(1).Ticks;
            //AxisUnit forces lets the axis know that we are plotting seconds
            //this is not always necessary, but it can prevent wrong labeling
            AxisUnit = TimeSpan.TicksPerSecond;

            

            SetAxisLimits(DateTime.Now);

            DataContext = this;
        }

        public ChartValues<MeasureModel> ChartValuesA { get; set; }
        public ChartValues<MeasureModel> ChartValuesB { get; set; }
        public Func<double, string> DateTimeFormatter { get; set; }
        public double AxisStep { get; set; }
        public double AxisUnit { get; set; }

        public double AxisXMax
        {
            get { return _axisxMax; }
            set
            {
                _axisxMax = value;
                OnPropertyChanged("AxisXMax");
            }
        }
        public double AxisXMin
        {
            get { return _axisxMin; }
            set
            {
                _axisxMin = value;
                OnPropertyChanged("AxisXMin");
            }
        }

        public double AxisY0Max
        {
            get { return _axisy0Max; }
            set
            {
                _axisy0Max = value;
                OnPropertyChanged("AxisY0Max");
            }
        }
        public double AxisY0Min
        {
            get { return _axisy0Min; }
            set
            {
                _axisy0Min = value;
                OnPropertyChanged("AxisY0Min");
            }
        }

        public double AxisY1Max
        {
            get { return _axisy1Max; }
            set
            {
                _axisy1Max = value;
                OnPropertyChanged("AxisY1Max");
            }
        }
        public double AxisY1Min
        {
            get { return _axisy1Min; }
            set
            {
                _axisy1Min = value;
                OnPropertyChanged("AxisY1Min");
            }
        }

        public string AxisY0Title
        {
            get { return _axisy0Title; }
            set
            {
                _axisy0Title = value;
                OnPropertyChanged("AxisY0Title");
            }
        }
        public string AxisY1Title
        {
            get { return _axisy1Title; }
            set
            {
                _axisy1Title = value;
                OnPropertyChanged("AxisY1Title");
            }
        }

        public double AxisY0Step
        {
            get { return _axisy0Step; }
            set
            {
                _axisy0Step = value;
                OnPropertyChanged("AxisY0Step");
            }
        }
        public double AxisY1Step
        {
            get { return _axisy1Step; }
            set
            {
                _axisy1Step = value;
                OnPropertyChanged("AxisY1Step");
            }
        }

        public void AddData(int index,  double value)
        {
            ChartValues<MeasureModel> chartValues;

            if (index == 0)
                chartValues = ChartValuesA;
            else
                chartValues = ChartValuesB;

            var now = DateTime.Now;


            chartValues.Add(new MeasureModel
            {
                DateTime = now,
                Value = value
            });

            SetAxisLimits(now);

            //lets only use the last 150 values
            if (chartValues.Count > MaxPoints) chartValues.RemoveAt(0);
        }

        private void SetAxisLimits(DateTime now)
        {
            now = RoundUp(now);
            AxisXMax = now.Ticks /*+ TimeSpan.FromMinutes(1).Ticks*/; // lets force the axis to be 1 second ahead
            AxisXMin = now.Ticks - TimeSpan.FromHours(12).Ticks; // and 12 hours behind
        }

        /// <summary>
        /// Rounds up to the nearest hour
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static DateTime RoundUp(DateTime dateTime)
        {
            var updated = dateTime.AddMinutes(60);
            return new DateTime(updated.Year, updated.Month, updated.Day,
                                 updated.Hour, 0, 0, dateTime.Kind);
        }

        #region INotifyPropertyChanged implementation

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    public class MeasureModel
    {
        public DateTime DateTime { get; set; }
        public double Value { get; set; }
    }
}


namespace Wpf.CartesianChart.Using_DateTime
{
    public class DateModel
    {
        public System.DateTime DateTime { get; set; }
        public double Value { get; set; }
    }
}