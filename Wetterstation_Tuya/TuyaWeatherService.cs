using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using TuyaAPI.Models;
using TuyaAPI;
using System.Timers;


/* Eventlog Error Codes
 * 
 * OK / Info
 * 1 Service started
 * 2 Service initiated
 * 3 Service stopped
 * 
 * 20 APRS Response
 * 21 APRS Dataline
 * 
 * 30 MQTT Connect
 * 31 MQTT Reconnect
 * 32 MQTT connected
 * 33 MQTT Init Complete
 * 
 * Errors
 * 37 MQTT (re-)connect
 * 38 MQTT Connection lost
 * 39 MQTT Client failed
 * 
 * 100 General Error in Loop
 * 101 Init failed
 * 102 Retry Init
 * 103 Get Tuya Device Info failed
 *
 */


namespace Wetterstation_Tuya
{
    public partial class TuyaWeatherService : ServiceBase
    {
        const string APPLICATION_NAME = "Weather Tuya API";

        string Software = "WeatherAPI 1.1.0";
        string BaseURL = Regions.URL_EU;

        string Client_ID = "";
        string Client_Secret = "";
        string deviceId = "";

        string URL_InfluxDB = "";
        string API_Key = "";
        string Org = "";
        string Bucket = "";
        string Table = "";
        string sensor = "";

        string Username = "";
        string Passcode = "";
        string URL_APRS = "";
        int Port = 14580;
        string Position = "";
        string Gateway = "rotate";

        int counter = 10;

        TuyaAPI.TuyaAPI tuyapi;
        InfluxDBAPI influxdbapi;
        APRS_API aprsapi;
        MQTTClient mqttClient;

        Timer timer = new Timer();
        System.Diagnostics.EventLog eventLog;


        public TuyaWeatherService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            eventLog = new System.Diagnostics.EventLog();
            eventLog.Source = "TuyaAPI";
            eventLog.WriteEntry("Service is started at " + DateTime.Now, EventLogEntryType.Information, 1);
            timer.Elapsed += new ElapsedEventHandler(OnElapsedTime);
            timer.Interval = 30000; //30 sek
            timer.Enabled = true;

            Init();

            eventLog.WriteEntry("Service initiated at " + DateTime.Now, EventLogEntryType.Information, 2);
        }

        void Init()
        {
            int errortry = 0;
            bool repeat = false;
            do
            {
                try
                {
                    string ProgramDataPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\" + APPLICATION_NAME;
                    FileAccess.CheckFolder(ProgramDataPath);
                    Settings settings = new Settings();
                    if (!FileAccess.CheckFile(ProgramDataPath + "\\settings.json"))
                    {
                        FileAccess.WriteJSONFile(ProgramDataPath + "\\settings.json", Helper.GetJson(settings));
                    }
                    settings = Helper.GetObject<Settings>(FileAccess.ReadJSONFile(ProgramDataPath + "\\settings.json"));

                    BaseURL = settings.BaseURL;
                    Client_ID = settings.Client_ID;
                    Client_Secret = settings.Client_Secret;
                    deviceId = settings.deviceId;

                    URL_InfluxDB = settings.URL_InfluxDB;
                    API_Key = settings.API_Key;
                    Org = settings.Org;
                    Bucket = settings.Bucket;
                    Table = settings.Table;
                    sensor = settings.sensor;

                    Username = settings.APRSUsername;
                    Passcode = settings.APRSPasscode;
                    URL_APRS = settings.APRSURL;
                    Port = settings.APRSPort;
                    Position = settings.Position;
                    Gateway = settings.APRSGateway;

                    repeat = false;
                    tuyapi = new TuyaAPI.TuyaAPI(Client_ID, Client_Secret, BaseURL);
                    if (influxdbapi == null)
                        influxdbapi = new InfluxDBAPI(URL_InfluxDB, API_Key, Bucket, Org);
                    if (aprsapi == null)
                        aprsapi = new APRS_API(URL_APRS, Port, Username, Passcode, Software, Position, Gateway, eventLog);
                    if (mqttClient == null)
                        //mqttClient.Disconnect();
                        mqttClient = new MQTTClient(eventLog, settings);

                    //var info = tuyapi.getDeviceInformation(deviceId);
                    //var deviceName = info.result.name;
                    //var isOnline = info.result.online;
                    //var uptime = info.result.update_time;
                    //var createtime = info.result.create_time;
                    //var activetime = info.result.active_time;
                    //var uid = info.result.uid;
                    //var users = tuyapi.getUserInfo(uid);

                    //var devlist1 = tuyapi.getDeviceList(uid);

                    //var dfn = tuyapi.getDeviceFunctions(deviceId);
                }
                catch (Exception ex)
                {
                    eventLog.WriteEntry("Init failed. " + ex.Message + "\r\n" + ex.StackTrace, EventLogEntryType.Error, 101);
                    //mqttClient?.Disconnect();
                    errortry++;
                    if (errortry < 3)
                    {
                        eventLog.WriteEntry("Retry init. Retry " + errortry.ToString(), EventLogEntryType.Information, 102);
                        repeat = true;
                    }
                }
            } while (repeat);
        }

        private void OnElapsedTime(object source, ElapsedEventArgs e)
        {
            loop();
        }

        protected void loop()
        {
            string LastStep = "Start";
            try
            {
                APRSData aprsData = new APRSData();
                MQTTData mqttData = new MQTTData();
                LastStep = "Init Vars";
                counter++;
                var ds = tuyapi.getDeviceStatus(deviceId);
                LastStep = "Get Device Status from TuyaAPI";
                string dataline = "";
                foreach (var dspropertie in ds.result)
                {
                    Console.WriteLine(dspropertie.code + ": " + dspropertie.value);
                    try
                    {
                        int value = Convert.ToInt32(dspropertie.value);
                        dataline = dataline + dspropertie.code + "=" + value + ",";
                        aprsData = helper.GetAPRSData(aprsData, dspropertie);
                        mqttData = helper.GetMQTTData(mqttData, dspropertie);
                    }
                    catch
                    {
                        dataline = dataline + dspropertie.code + "=\"" + dspropertie.value + "\",";
                    }
                    if (dspropertie.code == "wind_direct")
                    {
                        float winddirect = helper.GetWindDirection(dspropertie.value);
                        dataline = dataline + "wind_direct_degree=" + winddirect.ToString(System.Globalization.CultureInfo.InvariantCulture) + ",";
                        Console.WriteLine("wind_direct_degree: " + winddirect.ToString(System.Globalization.CultureInfo.InvariantCulture));
                        aprsData.wind_direct = Convert.ToInt16(winddirect);
                        mqttData.wind_direct = Convert.ToInt16(winddirect);
                    }
                    LastStep = "Dataline: " + dspropertie.code;
                    //dataline = dataline + dspropertie.code + "=" + dspropertie.value + ",";
                }
                LastStep = "DataLine Build";
                dataline = dataline.Remove(dataline.Length - 1, 1);
                influxdbapi.WriteCurrentData(Table, sensor, dataline, ds.t.ToUnixTimeMilliseconds().ToString());
                LastStep = "Sent data to InfluxDB";
                if (counter >= 10)
                {
                    try
                    {
                        var info = tuyapi.getDeviceInformation(deviceId);
                        LastStep = "Get Device Info from Tuya API";
                        if (info != null && info.result != null)
                        {
                            mqttData.online = info.result.online;
                            mqttData.LastUpdate = info.result.update_time;
                            mqttData.IP = info.result.ip;
                            LastStep = "Add Device Info to MQTT";
                        }
                        else
                        {
                            mqttData.online = true;
                            mqttData.LastUpdate = new DateTimeOffset(1970, 1, 1, 0, 0, 0, new TimeSpan(0));
                            mqttData.IP = "";
                            LastStep = "Add null Info to MQTT";
                        }
                    }
                    catch (Exception ex)
                    {
                        eventLog.WriteEntry(ex.Message + " \n " + ex.Source + " \n " + ex.StackTrace + " \n LastStep: " + LastStep, EventLogEntryType.Error, 103);
                    }

                    aprsapi.SendWeatherData(ds.t, aprsData.wind_direct, aprsData.wind_speed_avg, aprsData.wind_speed_gust, aprsData.temp, aprsData.rain, aprsData.humidity, aprsData.pressure, eventLog);
                    LastStep = "Pushed data to APRS";
                    mqttClient.PushData(mqttData);
                    LastStep = "Pushed data to MQTT";
                    counter = 0;
                }
            }
            catch (Exception ex)
            {
                eventLog.WriteEntry(ex.Message + " \n " + ex.Source + " \n " + ex.StackTrace + " \n LastStep: " + LastStep, EventLogEntryType.Error, 100);
                Init();
            }
        }

        protected override void OnStop()
        {
            eventLog.WriteEntry("Service stopped at " + DateTime.Now, EventLogEntryType.Information, 3);
        }
    }
}
