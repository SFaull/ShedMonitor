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

namespace WPF_Test
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MQTTManager mqttManager;
        private DispatcherTimer clockTimer;

        public MainWindow()
        {
            InitializeComponent();
            InitializeClock();
            InitializeMQTT();
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
                                + DateTime.Now.Minute.ToString("00") + ":"
                                + DateTime.Now.Second.ToString("00");
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

                decimal temp = e.Temperature;
                decimal hum = e.Humidity;
                Console.WriteLine("Temperature: {0}C, Humidity: {1}%", temp, hum);
            txtTemperature.Dispatcher.BeginInvoke((Action)(() => txtTemperature.Text = "Temperature: " + e.Temperature + "°C"));
            txtHumidity.Dispatcher.BeginInvoke( (Action)(() => txtHumidity.Text = "Humidity: " + e.Humidity + "%") );

        }
    }
}
