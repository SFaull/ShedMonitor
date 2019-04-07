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
using System.Threading;
using Wpf.CartesianChart.ConstantChanges;

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

        private double _axisMax;
        private double _axisMin;
        private double _trend;

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

                chart.AddData((double)e.Humidity);

                /*
                var now = DateTime.Now;
                chart.ChartValues.Add(new MeasureModel
                {
                    DateTime = now,
                    Value = (double)e.Temperature
                });

    */

                //chrtGraphTest.Series[1].Values.Add(new DateModel {DateTime = System.DateTime.Now, Value = (double)e.Humidity });
                //chrtGraphTest.Series[2].Values.Add(new DateModel {DateTime = System.DateTime.Now, Value = (double)e.Pressure });


                //SetAxisLimits(now);

                //lets only use the last 150 values
                //if (ChartValues.Count > 150) ChartValues.RemoveAt(0);

                // TODO: the chartvalues list is updated with the new values but the chart never plots any points? does this need to be wrapped in a usercontrol?

            }), (DispatcherPriority)10);
        }
    }
}
