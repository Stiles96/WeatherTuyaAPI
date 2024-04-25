using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uPLibrary.Networking.M2Mqtt;
using static uPLibrary.Networking.M2Mqtt.MqttClient;

namespace TuyaAPI
{
    public  class MQTTClient
    {
        string Username = "";
        string Password = "";
        string Host = "";
        int Port = 1883;
        string Topic = "";

        EventLog eventLog;
        bool connected = false;

        MqttClient MqttClient;
        string clientId = "TuyaAPI";

        public MQTTClient(EventLog eventLog = null, Models.Settings settings = null)
        {
            this.eventLog = eventLog;
            Username = settings.MQTTUsername;
            Password = settings.MQTTPassword;
            Host = settings.MQTTHost;
            Port = settings.MQTTPort;
            Topic = settings.MQTTTopic;
            try
            {
                if (eventLog != null)
                    eventLog.WriteEntry("Connect MQTT Client", EventLogEntryType.Information, 30);
                MqttClient = new MqttClient(Host);
                MqttClient.Connect(clientId, Username, Password, true, 15);
                connected = true;
                if (eventLog != null)
                    eventLog.WriteEntry("MQTT connected", EventLogEntryType.Information, 32);
            }
            catch (Exception ex)
            {
                if (eventLog != null)
                    eventLog.WriteEntry("Connect MQTT Client failed: " + ex.Message, EventLogEntryType.Error, 39);
            }
            InitHASensors();

            MqttClient.ConnectionClosed += (object sender, EventArgs e) =>
            {
                if (connected)
                {
                    if (eventLog != null)
                        eventLog.WriteEntry("Connect to MQTT lost, Reconnect ...", EventLogEntryType.Error, 38);
                    Reconnect();
                }
            };
        }

        public void InitHASensors()
        {
            PublishHAConfig("indoor_temp", "{ \"name\": \"Wetterstation Indoor Temperatur\", \"model\": \"TuyaAPI\", \"sw_version\":\"V01\", \"device_class\": \"temperature\", \"unit_of_measurement\": \"°C\", \"suggested_display_precision\":\"2\", \"state_topic\": \"" + Topic + "/indoor_temp\"}");
            PublishHAConfig("outdoor_temp", "{ \"name\": \"Wetterstation Outdoor Temperatur\", \"model\": \"TuyaAPI\", \"sw_version\":\"V01\", \"device_class\": \"temperature\", \"unit_of_measurement\": \"°C\", \"suggested_display_precision\":\"2\", \"state_topic\": \"" + Topic + "/outdoor_temp\"}");
            PublishHAConfig("indoor_humidity", "{ \"name\": \"Wetterstation Indoor Humidity\", \"model\": \"TuyaAPI\", \"sw_version\":\"V01\", \"device_class\": \"humidity\", \"unit_of_measurement\": \"%\", \"state_topic\": \"" + Topic + "/indoor_humidity\"}");
            PublishHAConfig("outdoor_humidity", "{ \"name\": \"Wetterstation Outdoor Humidity\", \"model\": \"TuyaAPI\", \"sw_version\":\"V01\", \"device_class\": \"humidity\", \"unit_of_measurement\": \"%\", \"state_topic\": \"" + Topic + "/outdoor_humidity\"}");
            PublishHAConfig("pressure", "{ \"name\": \"Wetterstation Pressure\", \"model\": \"TuyaAPI\", \"sw_version\":\"V01\", \"device_class\": \"atmospheric_pressure\", \"unit_of_measurement\": \"hPa\", \"state_topic\": \"" + Topic + "/pressure\"}");
            PublishHAConfig("rain", "{ \"name\": \"Wetterstation Rain Today\", \"model\": \"TuyaAPI\", \"sw_version\":\"V01\", \"device_class\": \"precipitation\", \"unit_of_measurement\": \"mm\", \"state_topic\": \"" + Topic + "/rain\"}");
            PublishHAConfig("wind_direct", "{ \"name\": \"Wetterstation Wind Direction\", \"model\": \"TuyaAPI\", \"sw_version\":\"V01\", \"unit_of_measurement\": \"°\", \"state_topic\": \"" + Topic + "/wind_direct\"}");
            PublishHAConfig("wind_speed", "{ \"name\": \"Wetterstation Wind Speed\", \"model\": \"TuyaAPI\", \"sw_version\":\"V01\", \"device_class\": \"wind_speed\", \"unit_of_measurement\": \"km/h\", \"state_topic\": \"" + Topic + "/wind_speed\"}");
            PublishHAConfig("dew_point", "{ \"name\": \"Wetterstation Dew Point\", \"model\": \"TuyaAPI\", \"sw_version\":\"V01\", \"device_class\": \"temperature\", \"unit_of_measurement\": \"°C\", \"suggested_display_precision\":\"2\", \"state_topic\": \"" + Topic + "/dew_point\"}");
            PublishHAConfig("bright", "{ \"name\": \"Wetterstation Bright\", \"model\": \"TuyaAPI\", \"sw_version\":\"V01\", \"device_class\": \"illuminance\", \"unit_of_measurement\": \"lx\", \"state_topic\": \"" + Topic + "/bright\"}");
            PublishHAConfig("online", "{ \"name\": \"Wetterstation Online\", \"model\": \"TuyaAPI\", \"sw_version\":\"V01\", \"state_topic\": \"" + Topic + "/online\"}");
            PublishHAConfig("last_update", "{ \"name\": \"Wetterstation Last Update\", \"model\": \"TuyaAPI\", \"sw_version\":\"V01\", \"device_class\": \"timestamp\", \"state_topic\": \"" + Topic + "/last_update\"}");
            PublishHAConfig("ip", "{ \"name\": \"Wetterstation IP Address\", \"model\": \"TuyaAPI\", \"sw_version\":\"V01\", \"state_topic\": \"Wetterstation/ip\"}");

            if (eventLog != null)
                eventLog.WriteEntry("Init HA Sensors completed.", EventLogEntryType.Information, 33);
        }

        void PublishHAConfig(string Name, string payload)
        {
            MqttClient.Publish("homeassistant/sensor/" + Name + "/config", Encoding.UTF8.GetBytes(payload), 0, true);
        }

        void Reconnect()
        {
            if (eventLog != null)
                eventLog.WriteEntry("Re-Connect MQTT Client", EventLogEntryType.Information, 31);
            MqttClient.Connect(clientId, Username, Password, false, 15);
            connected = true;
            InitHASensors();
        }

        public void PushData(Models.MQTTData mqttdata)
        {
            if (!MqttClient.IsConnected)
            {
                if (eventLog != null)
                    eventLog.WriteEntry("MQTT not connected, (Re-)connect ...", EventLogEntryType.Warning, 37);
                Reconnect();
            }
            MqttClient.Publish(Topic + "/indoor_temp", Encoding.UTF8.GetBytes(mqttdata.indoor_temp.ToString().Replace(",", ".")), 0, false);
            MqttClient.Publish(Topic + "/indoor_humidity", Encoding.UTF8.GetBytes(mqttdata.indoor_humidity.ToString()), 0, false);
            MqttClient.Publish(Topic + "/outdoor_temp", Encoding.UTF8.GetBytes(mqttdata.outdoor_temp.ToString().Replace(",", ".")), 0, false);
            MqttClient.Publish(Topic + "/outdoor_humidity", Encoding.UTF8.GetBytes(mqttdata.outdoor_humidity.ToString()), 0, false);
            MqttClient.Publish(Topic + "/pressure", Encoding.UTF8.GetBytes(mqttdata.pressure.ToString()), 0, false);
            MqttClient.Publish(Topic + "/rain", Encoding.UTF8.GetBytes(mqttdata.rain.ToString().Replace(",", ".")), 0, false);
            MqttClient.Publish(Topic + "/wind_direct", Encoding.UTF8.GetBytes(mqttdata.wind_direct.ToString()), 0, false);
            MqttClient.Publish(Topic + "/wind_speed", Encoding.UTF8.GetBytes(mqttdata.wind_speed_avg.ToString()), 0, false);
            MqttClient.Publish(Topic + "/dew_point", Encoding.UTF8.GetBytes(mqttdata.dew_point.ToString().Replace(",", ".")), 0, false);
            MqttClient.Publish(Topic + "/bright", Encoding.UTF8.GetBytes(mqttdata.bright.ToString()), 0, false);
            MqttClient.Publish(Topic + "/online", Encoding.UTF8.GetBytes(mqttdata.online.ToString()), 0, false);
            MqttClient.Publish(Topic + "/last_update", Encoding.UTF8.GetBytes(mqttdata.LastUpdate.ToString("O")), 0, false);
            MqttClient.Publish(Topic + "/ip", Encoding.UTF8.GetBytes(mqttdata.IP), 0, false);
        }

        public void Disconnect()
        {
            connected = false;
            MqttClient.Disconnect();
        }
    }
}
