#include <Wire.h>
#include <Adafruit_Sensor.h>
#include <Adafruit_BME280.h>
#include <WiFiManager.h>
#include <ESP8266WiFi.h>
#include <PubSubClient.h>
#include <ArduinoOTA.h>
#include <WifiUDP.h>
#include <String.h>
#include <stdio.h>
#include <string.h>
#include "config.h"

#define PUBLISH_PERIOD 5000 // 5 seconds
#define SEALEVELPRESSURE_HPA (1013.25)

Adafruit_BME280 bme; // I2C
WiFiClient espClient;
PubSubClient client(espClient);

const char* deviceName              = "ShedMonitor";
const char* MQTTtopic               = "ShedMonitor/#";
const char* MQTTtopic_comms         = "ShedMonitor/Comms";
const char* MQTTtopic_humidity      = "ShedMonitor/Humidity";
const char* MQTTtopic_temperature   = "ShedMonitor/Temperature";
const char* MQTTtopic_pressure      = "ShedMonitor/Pressure";
const char* MQTTtopic_altitude      = "ShedMonitor/Altitude";
const char* MQTTtopic_rawdata       = "ShedMonitor/RawData";

unsigned long runTime         = 0,
              publishTimer    = 0;

float humidity = 0;
float temperature = 0;
float pressure = 0;
float altitude = 0;

void initOTA(void)
{
  ArduinoOTA.onStart([]() {
    Serial.println("OTA Update Started");
  });
  ArduinoOTA.onEnd([]() {
    Serial.println("\nOTA Update Complete");
  });
  ArduinoOTA.onProgress([](unsigned int progress, unsigned int total) {
    Serial.printf("Progress: %u%%\r", (progress / (total / 100)));
  });
  ArduinoOTA.onError([](ota_error_t error) {
    Serial.printf("Error[%u]: ", error);
    if (error == OTA_AUTH_ERROR) Serial.println("Auth Failed");
    else if (error == OTA_BEGIN_ERROR) Serial.println("Begin Failed");
    else if (error == OTA_CONNECT_ERROR) Serial.println("Connect Failed");
    else if (error == OTA_RECEIVE_ERROR) Serial.println("Receive Failed");
    else if (error == OTA_END_ERROR) Serial.println("End Failed");
  });
  ArduinoOTA.begin();
}

void initSensor()
{
  // default settings
  // (you can also pass in a Wire library object like &Wire2)
  bool status = bme.begin(0x76);  
  if (!status) {
    Serial.println("Could not find a valid BME280 sensor, check wiring!");
    while (1);
  }
}

/* pass this function a pointer to an unsigned long to store the start time for the timer */
void setTimer(unsigned long *startTime)
{
  runTime = millis();    // get time running in ms
  *startTime = runTime;  // store the current time
}

/* call this function and pass it the variable which stores the timer start time and the desired expiry time
   returns true fi timer has expired */
bool timerExpired(unsigned long startTime, unsigned long expiryTime)
{
  runTime = millis(); // get time running in ms
  if ( (runTime - startTime) >= expiryTime )
    return true;
  else
    return false;
}

void publishReadings(void)
{
  /* publish */
  char raw[30];
  sprintf(raw, "%s,%s,%s,%s", String(humidity).c_str(), String(temperature).c_str(), String(pressure).c_str(), String(altitude).c_str());
  
  client.publish(MQTTtopic_rawdata, raw);
  client.publish(MQTTtopic_humidity, String(humidity).c_str());
  client.publish(MQTTtopic_temperature, String(temperature).c_str());
  client.publish(MQTTtopic_pressure, String(pressure).c_str());
  client.publish(MQTTtopic_altitude, String(altitude).c_str());
  
  Serial.println(raw);
}

void callback(char* topic, byte* payload, unsigned int length)
{
  Serial.print("Message arrived [");
  Serial.print(topic);
  Serial.print("] ");

  /* --------------- Print incoming message to serial ------------------ */
  char input[length + 1];
  for (int i = 0; i < length; i++)
    input[i] = (char)payload[i];  // store payload as char array
  input[length] = '\0'; // dont forget to add a termination character

  Serial.print("MQTT message received: ");
  Serial.println(input);

  if (strcmp(topic, MQTTtopic_comms)==0)
  {    
    if(strcmp(input,"*IDN?")==0)
    {
      // give info
    }
  }
}

void reconnect() {
  // Loop until we're reconnected
  while (!client.connected())
  {
    Serial.print("Attempting MQTT connection... ");
    // Attempt to connect
    if (client.connect(deviceName, MQTTuser, MQTTpassword))
    {
      Serial.println("Connected");
      // Once connected, publish an announcement...
      client.publish(MQTTtopic_comms, "Connected");  // potentially not necessary
      // ... and resubscribe
      client.subscribe(MQTTtopic_comms);
    }
    else
    {
      Serial.print("failed, rc=");
      Serial.print(client.state());
      Serial.print(" try again in 5 seconds");
      // Wait 5 seconds before retrying
      delay(5000);
    }
  }
}

void readSensors(void)
{
  temperature = bme.readTemperature();
  humidity = bme.readHumidity();
  pressure = bme.readPressure();
  altitude = bme.readAltitude(SEALEVELPRESSURE_HPA);  // approximate
}


void setup()
{
  Serial.begin(115200);

  /* Setup WiFi and MQTT */
  //Local intialization. Once its business is done, there is no need to keep it around
  WiFiManager wifiManager;
  wifiManager.autoConnect(deviceName);

  client.setServer(MQTTserver, MQTTport);
  client.setCallback(callback);

  initOTA();
  initSensor();

  setTimer(&publishTimer);
}

void loop()
{
  /* Check WiFi Connection */
  if (!client.connected())
    reconnect();
  client.loop();
  ArduinoOTA.handle();

  if (timerExpired(publishTimer, PUBLISH_PERIOD))  // get the time every 5 seconds
  {
    setTimer(&publishTimer);  // reset timer
    readSensors();
    publishReadings();
  }
}



