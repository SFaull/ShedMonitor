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
using LiveCharts.Wpf;
using Wpf.CartesianChart.ConstantChanges;

namespace SmartMonitorApp
{
    public partial class UserControlRHT : UserControl
    {
        private MQTTManager mqttManager;

        public UserControlRHT()
        {
            InitializeComponent();
            InitializeGuages();
            InitializeGraphs();
        }

        /// <summary>
        /// Initialise the visual gauges
        /// </summary>
        private void InitializeGuages()
        {
            guageHumidity.FromColor = Color.FromRgb(0, 0, 255);
            guageHumidity.ToColor = Color.FromRgb(255, 0, 0);
            guageTemperature.FromColor = Color.FromRgb(0, 0, 255);
            guageTemperature.ToColor = Color.FromRgb(255, 0, 0);
            guagePressure.FromColor = Color.FromRgb(0, 0, 255);
            guagePressure.ToColor = Color.FromRgb(255, 0, 0);
        }

        /// <summary>
        /// Initialise the visual graphs
        /// </summary>
        private void InitializeGraphs()
        {
            // Humidity
            graphRHT.AxisY0Title = "Humidity (%)";
            graphRHT.AxisY0Max = 100;
            graphRHT.AxisY0Min = 0;
            graphRHT.AxisY0Step = 10;

            // Temperature
            graphRHT.AxisY1Title = "Temperature (degC)";
            graphRHT.AxisY1Max = 80;
            graphRHT.AxisY1Min = -20;
            graphRHT.AxisY1Step = 10;

            // Current
            graphEnergy.AxisY0Title = "Current (A)";
            graphEnergy.AxisY0Max = 15;
            graphEnergy.AxisY0Min = 0;
            graphEnergy.AxisY0Step = 1;

            // Power
            graphEnergy.AxisY1Title = "Power (kW)";
            graphEnergy.AxisY1Max = 3;
            graphEnergy.AxisY1Min = 0;
            graphEnergy.AxisY1Step = 3;
        }

        public void ImportMQTT(MQTTManager _mqttManager)
        {
            mqttManager = _mqttManager;
            mqttManager.MessageReceived += MQTTManager_MessageReceived;
        }

        /// <summary>
        /// Event handler for MQTT massage received
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MQTTManager_MessageReceived(object sender, SensorEventArgs e)
        {
            Gauge guage;
            ConstantChangesChart graph;
            int index = 0; // TODO make this better
            double value = (double)e.SensorValue;

            switch (e.SensorType)
            {
                case "Humidity":
                    guage = guageHumidity;
                    graph = graphRHT;
                    index = 0;
                    break;
                case "Temperature":
                    guage = guageTemperature;
                    graph = graphRHT;
                    index = 1;
                    break;
                case "Pressure":
                    guage = guagePressure;
                    value /= 1000;  // reported in Pascals lets change it to kPascals
                    graph = null;
                    break;
                case "Altitude":
                    // do nothing
                    return;
                case "Current":
                    guage = guageCurrent;
                    graph = graphEnergy;
                    index = 0;
                    break;
                case "Power":
                    guage = guagePower;
                    graph = graphEnergy;
                    value /= 1000;  // reported in watts, lets change it to kW
                    index = 1;
                    break;
                default:
                    // TODO log error
                    return;
            }

            // update the guage and graph
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (guage != null)
                    guage.Value = value;

                if (graph != null)
                    graph.AddData(index, value);
            }), (DispatcherPriority)10);
        }
    }
}
