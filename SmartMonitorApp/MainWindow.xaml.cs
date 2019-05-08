using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using Wpf.CartesianChart.ConstantChanges;
using SmartMonitorApp.Properties;

namespace SmartMonitorApp
{
    public partial class MainWindow : Window
    {
        private MQTTManager mqttManager;
        private UserControl userControlHome;
        private UserControl userControlRHT;
        private UserControl userControlEnergy;

        /// <summary>
        /// Constructor for the main window
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            InitClock();
            InitializeMQTT();
            InitializeUserControls();

            // set default page
            GridMain.Children.Add(userControlHome);
        }

        private void InitializeUserControls()
        {
            userControlHome = new UserControlHome();
            userControlRHT = new UserControlRHT();
            userControlEnergy = new UserControlEnergy();
        }

        public void InitClock()
        {
            UpdateClock(null, null);
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(200);
            timer.Tick += new EventHandler(UpdateClock);
            timer.Start();
        }


        void UpdateClock(object sender, EventArgs e)
        {
            txtTime.Text = DateTime.Now.ToString();
        }

        /// <summary>
        /// On clicking the icon to open the menu, open the menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonOpenMenu_Click(object sender, RoutedEventArgs e)
        {
            ButtonCloseMenu.Visibility = Visibility.Visible;
            ButtonOpenMenu.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// On clicking the icon to close the menu, close the menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonCloseMenu_Click(object sender, RoutedEventArgs e)
        {
            ButtonCloseMenu.Visibility = Visibility.Collapsed;
            ButtonOpenMenu.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// On clicking a menu item, change the view contents
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ListViewMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GridMain.Children.Clear();

            switch (((ListViewItem)((ListView)sender).SelectedItem).Name)
            {
                case "ItemHome":
                    GridMain.Children.Add(userControlHome);
                    break;
                case "ItemRHT":
                    GridMain.Children.Add(userControlRHT);
                    break;
                case "ItemPower":
                    GridMain.Children.Add(userControlEnergy);
                    break;
                default:
                    MessageBox.Show("Unknown selection");
                    break;
            }


             // TODO, close the menu if its open
        }

        /// <summary>
        /// On clicking popup box, perform the relevant task
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PopupBox_ItemSelected(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;

            switch(button.Content)
            {
                case "Toggle FullScreen":
                    if (WindowState != WindowState.Maximized)
                        WindowState = WindowState.Maximized;
                    else
                        WindowState = WindowState.Normal;
                    break;
                case "Settings":
                    MessageBox.Show("Settings Page Not Implemented");
                    break;
                case "Help":
                    MessageBox.Show("Help Page Not Implemented");
                    break;
                case "Test":
                    EnableAlarm(true);
                    break;
                case "Close":
                    this.Close();
                    break;
                default:
                    MessageBox.Show("Unknown selection");
                    break;
            }
        }

        /// <summary>
        /// Initialise the MQTT manager
        /// </summary>
        private void InitializeMQTT()
        {
            mqttManager = new MQTTManager();
            bool connected = mqttManager.Connect(Settings.Default.MqttUsername, Settings.Default.MqttPassword);
            if (connected)
                mqttManager.MessageReceived += MQTTManager_MessageReceived;
            else
                DisconnectMQTT();
        }

        /// <summary>
        /// Handler for Window close event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            DisconnectMQTT();
        }


        private void DisconnectMQTT()
        {
            if (mqttManager != null)
            {
                mqttManager.Disconnect();
                mqttManager = null;
            }
        }

        /// <summary>
        /// Event handler for MQTT massage received
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MQTTManager_MessageReceived(object sender, SensorEventArgs e)
        {
            double value = (double)e.SensorValue;
            string sensorType = e.SensorType;
            
            // determine which usercontrol to update based on the sensortype
            switch (sensorType)
            {
                case "Humidity":
                case "Temperature":
                case "Pressure":
                case "Altitude":
                case "Power":
                case "Current":
                    ((UserControlRHT)userControlRHT).UpdateSensorValue(sensorType, value);
                    break;
                default:
                    Console.WriteLine("Unknown Sensor Type: {0}", sensorType);
                    break;
            }

            // special sensor types which require alarms
            if (sensorType.Equals("Current"))
               EnableAlarm(value > 10); // Enforce a 10A alarm threshold

        }

        /// <summary>
        /// for info on dialog boxes, see https://intellitect.com/material-design-in-xaml-dialog-host/
        /// </summary>
        /// <returns></returns>
        private void EnableAlarm(bool enable)
        {
            var alarm = new Alarm()
            {
                AlarmType = "Over Current Alarm!",
                AlarmDescription = "Current has exceeded the alarm threshold"
            };

            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (enable)
                {
                    DialogHost.Show(alarm);
                    Audio.PlayLoop();
                }
                else
                {
                    DialogHost.CloseDialogCommand.Execute(null, null);
                    Audio.StopLoop();
                }
            }), (DispatcherPriority)10);
            
        }



        /// <summary>
        /// Alarm object (type and alarm description)
        /// </summary>
        public class Alarm
        {
            public string AlarmType { get; set; }
            public string AlarmDescription { get; set; }
        }
    }
}