using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace WPF_Test
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
        MqttClient client;
        private string clientId;
        private static string username = "jnozycds";
        private static string password = "VYjomWiuoISU";
        private static int port = Convert.ToInt32(16601);

        #region Events

        public event EventHandler<SensorEventArgs> MessageReceived;

        protected void OnMessageReceived(SensorEventArgs args)
        {
            MessageReceived?.BeginInvoke(this, args, null, null);
        }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public MQTTManager()
        {

        }

        public void Connect()
        {
            string BrokerAddress = "m21.cloudmqtt.com";
            client = new MqttClient(BrokerAddress, port, false, null, null, MqttSslProtocols.None);

            // register a callback-function (we have to implement, see below) which is called by the library when a message was received
            client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;
            string[] subscriptions = { "ShedMonitor/RawData",
                                       "ShedMonitor/Temperature",
                                       "ShedMonitor/Humidity",
                                       "Shed/Current",
                                       "Shed/Power"  };
            byte[] qos = { 0,0,0,0,0 };
            client.Subscribe(subscriptions, qos);

            // use a unique id as client id, each time we start the application
            clientId = Guid.NewGuid().ToString();

            client.Connect(clientId, username, password);
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
