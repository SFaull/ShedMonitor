using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace SmartMonitorApp
{
    public class SensorEventArgs : EventArgs
    {
        public string SensorType { private set; get; }
        public decimal SensorValue { private set; get; }

        public SensorEventArgs(string type, decimal value)
        {
            SensorType = type;
            SensorValue = value;
        }
    }

    public class MQTTManager
    {
        private MqttClient client;
        private string clientId;
        private static int Port = Convert.ToInt32(16601);
        const string BrokerAddress = "m21.cloudmqtt.com";

        #region Events

        public event EventHandler<SensorEventArgs> MessageReceived;

        protected void OnMessageReceived(SensorEventArgs args)
        {
            MessageReceived?.BeginInvoke(this, args, null, null);
        }

        #endregion

        private static  MQTTManager instance;

        public static MQTTManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new MQTTManager();
                return instance;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        private MQTTManager()
        {
            // Prevent outside instantiation
        }

        ~MQTTManager()
        {
            Console.WriteLine("MQTTManager Disposed");
        }

        public bool Connect(string username, string password)
        {
            if (this.IsConnected())
                this.Disconnect();

            client = new MqttClient(BrokerAddress, Port, false, null, null, MqttSslProtocols.None);

            // register a callback-function (we have to implement, see below) which is called by the library when a message was received
            client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;
            string[] subscriptions = { "ShedMonitor/RawData",
                                       "ShedMonitor/Temperature",
                                       "ShedMonitor/Humidity",
                                       "ShedMonitor/Pressure",
                                       "ShedMonitor/Altitude",
                                       "Shed/Current",
                                       "Shed/Power"  };
            byte[] qos = { 0,0,0,0,0,0,0 };
            client.Subscribe(subscriptions, qos);

            // use a unique id as client id, each time we start the application
            clientId = Guid.NewGuid().ToString();

            byte retVal = client.Connect(clientId, username, password);
            Console.WriteLine("MQTT Connect: " + retVal);

            if (retVal != 0)
            {
                // error connecting
                this.Disconnect();
                return false;
            }

            return true;
        }

        public void Disconnect()
        {
            client.MqttMsgPublishReceived -= client_MqttMsgPublishReceived;
            client.Disconnect();
            Console.WriteLine("MQTT Disconnected");
            client = null;
        }

        public bool IsConnected()
        {
            if (client != null)
                return client.IsConnected;
            return false;
        } 

        public bool Publish(string topic, string message)
        {
            try
            {
                client.Publish(topic, Encoding.UTF8.GetBytes(message), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, true);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: unable to publish message... " + e.Message);
                return false;
            }
        }


        private void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            try
            {
                string topic = e.Topic;
                string receivedMessage = Encoding.UTF8.GetString(e.Message);
                Console.WriteLine("Topic: {0}, Message:{1}", topic, receivedMessage);

                List<string> topicSplit = topic.Split('/').ToList();

                if(topicSplit.Count != 2)
                {
                    // unexpected message, discard. TODO: log error
                    return;
                }

                string room = topicSplit[0]; // should be either Shed or ShedMonitor - this needs to be made consistent and then checked.
                string sensorType = topicSplit[1];

                List<string> messages = receivedMessage.Split(',').ToList();
                bool conversionSuccess = decimal.TryParse(receivedMessage, out decimal value);

                if(!conversionSuccess)
                {
                    // unexpected message, discard. TODO: log error
                    return;
                }

                OnMessageReceived(new SensorEventArgs(sensorType, value));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: unable to parse received message. Message: {0}, Exception: {1} ", e.Message, ex.Message);
            }
        }
    }
}
