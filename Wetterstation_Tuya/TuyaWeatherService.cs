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

namespace Wetterstation_Tuya
{
    public partial class TuyaWeatherService : ServiceBase
    {
        string BaseURL = Regions.URL_EU;
		string Client_ID = "TuyaClientID";
		string Client_Secret = "TuyaClientSecret";
		string deviceId = "TuyaDeviceID_WeatherStation";

		string URL_InfluxDB = "http://influxdb.local:8086";
		string API_Key = "InfluxDBAPIKey";
		string Org = "InfluxDBOrg";
		string Bucket = "weather";
		string Table = "Wetterstation";
		string sensor = "tuya";

		string Username = "Callsign-13";
		string Passcode = "Passcode";
		string Software = "WeatherAPI 1.0.0";
		string URL_APRS = "rotate.aprs.net";
		int Port = 14580;
		string Position = "Loacation";
		string Gateway = "rotate";

        int counter = 10;

        TuyaAPI.TuyaAPI tuyapi;
        InfluxDBAPI influxdbapi;
        APRS_API aprsapi;

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

            int errortry = 0;
            bool repeat = false;
            do
            {
                try
                {
                    repeat = false;
                    tuyapi = new TuyaAPI.TuyaAPI(Client_ID, Client_Secret, BaseURL);
                    influxdbapi = new InfluxDBAPI(URL_InfluxDB, API_Key, Bucket, Org);
                    aprsapi = new APRS_API(URL_APRS, Port, Username, Passcode, Software, Position, Gateway, eventLog);

                    var info = tuyapi.getDeviceInformation(deviceId);
                    var deviceName = info.result.name;
                    var isOnline = info.result.online;
                    var uptime = info.result.update_time;
                    var createtime = info.result.create_time;
                    var activetime = info.result.active_time;
                    var uid = info.result.uid;
                    var users = tuyapi.getUserInfo(uid);

                    var devlist1 = tuyapi.getDeviceList(uid);

                    var dfn = tuyapi.getDeviceFunctions(deviceId);
                }
                catch (Exception ex)
                {
                    eventLog.WriteEntry("Init failed. " + ex.Message, EventLogEntryType.Error, 101);
                    errortry++;
                    if (errortry < 3)
                    {
                        eventLog.WriteEntry("Retry init. Retry " + errortry.ToString(), EventLogEntryType.Information, 102);
                        repeat = true;
                    }
                }
            } while (repeat);


            eventLog.WriteEntry("Service initiated at " + DateTime.Now, EventLogEntryType.Information, 2);

        }

        private void OnElapsedTime(object source, ElapsedEventArgs e)
        {
            loop();
        }

        protected void loop()
        {
            try
            {
                APRSData aprsData = new APRSData();
                counter++;
                var ds = tuyapi.getDeviceStatus(deviceId);
                string dataline = "";
                foreach (var dspropertie in ds.result)
                {
                    Console.WriteLine(dspropertie.code + ": " + dspropertie.value);
                    try
                    {
                        int value = Convert.ToInt32(dspropertie.value);
                        dataline = dataline + dspropertie.code + "=" + value + ",";
                        aprsData = helper.GetAPRSData(aprsData, dspropertie);
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
                    }
                    //dataline = dataline + dspropertie.code + "=" + dspropertie.value + ",";
                }
                dataline = dataline.Remove(dataline.Length - 1, 1);
                influxdbapi.WriteCurrentData(Table, sensor, dataline, ds.t.ToUnixTimeMilliseconds().ToString());
                if (counter >= 10)
                {
                    aprsapi.SendWeatherData(ds.t, aprsData.wind_direct, aprsData.wind_speed_avg, aprsData.wind_speed_gust, aprsData.temp, aprsData.rain, aprsData.humidity, aprsData.pressure, eventLog);
                    counter = 0;
                }
            }
            catch (Exception ex)
            {
                eventLog.WriteEntry(ex.Message + " \n " + ex.Source + " \n " + ex.StackTrace, EventLogEntryType.Error, 100);
            }
        }

        protected override void OnStop()
        {
            eventLog.WriteEntry("Service stopped at " + DateTime.Now, EventLogEntryType.Information, 3);
        }
    }
}
