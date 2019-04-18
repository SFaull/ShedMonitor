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
            InitializeMQTT();
            InitialiseUserControls();
        }

        private void InitialiseUserControls()
        {
            userControlHome = new UserControlHome();
            userControlRHT = new UserControlRHT();
            userControlEnergy = new UserControlEnergy();

            ((UserControlRHT)userControlRHT).ImportMQTT(mqttManager);
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
                case "FullScreen":
                    MessageBox.Show("Fullscreen Not Implemented");  // TODO hide top and left navbars and fullscreen
                    break;
                case "Settings":
                    MessageBox.Show("Settings Page Not Implemented");
                    break;
                case "Help":
                    MessageBox.Show("Help Page Not Implemented");
                    break;
                case "Test":
                    EnableAlarmAsync();
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
            mqttManager.Connect();
            mqttManager.MessageReceived += MQTTManager_MessageReceived;
        }

        /// <summary>
        /// Handler for Window close event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, CancelEventArgs e)
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
            if (e.SensorType == "Current")
            {
                if (e.SensorValue > 12)
                    EnableAlarmAsync();
            }
        }

        // for info on dialog boxes, see https://intellitect.com/material-design-in-xaml-dialog-host/
        private async Task EnableAlarmAsync()
        {
            var alarm = new Alarm()
            {
                AlarmType = "Over Current Alarm!",
                AlarmDescription = "Current has exceeded the alarm threshold"
            };
            await DialogHost.Show(alarm);
        }

        public class Alarm
        {
            public string AlarmType { get; set; }
            public string AlarmDescription { get; set; }
        }
    }
}