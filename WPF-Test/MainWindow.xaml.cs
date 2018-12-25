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
using System.Windows.Threading;
using Wpf.Gauges;
using LiveCharts.Wpf;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Configurations;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace WPF_Test
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MQTTManager mqttManager;
        private DispatcherTimer clockTimer;
        //private SeriesCollection scol;

        public MainWindow()
        {
            InitializeComponent();
            InitializeClock();
            InitializeMQTT();

            guageHumidity.FromColor = Color.FromRgb(0, 0, 255);
            guageHumidity.ToColor = Color.FromRgb(255, 0, 0);
            guageTemperature.FromColor = Color.FromRgb(0, 0, 255);
            guageTemperature.ToColor = Color.FromRgb(255, 0, 0);
            guagePressure.FromColor = Color.FromRgb(0, 0, 255);
            guagePressure.ToColor = Color.FromRgb(255, 0, 0);


            InitGraph();
        }

        /// <summary>
        /// Start the clock timer
        /// </summary>
        private void InitializeClock()
        {
            clockTimer = new DispatcherTimer();
            clockTimer.Interval = TimeSpan.FromSeconds(1.0);
            clockTimer.Start();
            clockTimer.Tick += UpdateClock;
            UpdateClock(null,null); // call the function imediatelly to update on form load rather than 1 second in
        }

        /// <summary>
        /// Refresh the clock time on a timer event
        /// </summary>
        /// <param name="s"></param>
        /// <param name="a"></param>
        private void UpdateClock(object s, EventArgs a)
        {
            txtClock.Text = "" + DateTime.Now.Hour.ToString("00") + ":"
                                + DateTime.Now.Minute.ToString("00");

             // + ":" + DateTime.Now.Second.ToString("00");
        }

        /// <summary>
        /// Initialise the MQTT manager
        /// </summary>
        private void InitializeMQTT()
        {
            mqttManager = new MQTTManager();
            mqttManager.Connect();
            mqttManager.MessageReceived += MQTTManager_MessageReceived;
        }

        public SeriesCollection SeriesCollection { get; set; }
        public Func<double, string> Formatter { get; set; }



        private void InitGraph()
        {
            /*
            var dayConfig = Mappers.Xy<DateModel>()
    .X(dayModel => (double)dayModel.DateTime.Ticks / TimeSpan.FromHours(1).Ticks)
    .Y(dayModel => dayModel.Value);
    */
            var dayConfig = Mappers.Xy<DateModel>()
.X(dayModel => (double)dayModel.DateTime.Ticks / TimeSpan.FromHours(12).Ticks)
.Y(dayModel => dayModel.Value);

            SeriesCollection = new SeriesCollection(dayConfig)
            {
                new LineSeries
                {
                    Title = "Temp",
                    Values = new ChartValues<DateModel>{ },
                    PointGeometrySize = 15
                },
                new LineSeries
                {
                    Title = "Humidity",
                    Values = new ChartValues<DateModel>{ },
                    PointGeometrySize = 15
                },
                new LineSeries
                {
                    Title = "Pressure",
                    Values = new ChartValues<DateModel>{ },
                    PointGeometrySize = 15
                }
            };

            Formatter = value => new System.DateTime((long)(value * TimeSpan.FromHours(1).Ticks)).ToString("t");

            DataContext = this;
        }


        // SEE https://lvcharts.net/App/examples/v1/wpf/Constant%20Changes for moving time axis


        /// <summary>
        /// Event handler for MQTT massage received
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MQTTManager_MessageReceived(object sender, SensorEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                guageHumidity.Value = (double)e.Humidity;
                guageTemperature.Value = (double)e.Temperature;
                guagePressure.Value = (double)e.Pressure;

                SeriesCollection[0].Values.Add(new DateModel {DateTime = System.DateTime.Now, Value = (double)e.Temperature });
                SeriesCollection[1].Values.Add(new DateModel {DateTime = System.DateTime.Now, Value = (double)e.Humidity });
                //SeriesCollection[2].Values.Add(new DateModel {DateTime = System.DateTime.Now, Value = (double)e.Pressure });
            }), (DispatcherPriority)10);
        }
    }

    public class DateModel
    {
        public System.DateTime DateTime { get; set; }
        public double Value { get; set; }
    }


}
