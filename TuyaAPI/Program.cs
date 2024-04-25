using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TuyaAPI.Models;
using uPLibrary.Networking.M2Mqtt.Messages;


namespace TuyaAPI
{
    internal class Program
    {
        static void Main(string[] args)
        {
            const string APPLICATION_NAME = "Weather Tuya API";
            Console.Title = APPLICATION_NAME;
            string ProgramDataPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\" + APPLICATION_NAME;
            FileAccess.CheckFolder(ProgramDataPath);
            Settings settings = Helper.GetObject<Settings>(FileAccess.ReadJSONFile(ProgramDataPath + "\\settings.json"));

            string Software = "WeatherAPI 1.1.0";
            string BaseURL = Regions.URL_EU;

            BaseURL = settings.BaseURL;
            string Client_ID = settings.Client_ID;
            string Client_Secret = settings.Client_Secret;
            string deviceId = settings.deviceId;

            string URL_InfluxDB = settings.URL_InfluxDB;
            string API_Key = settings.API_Key;
            string Org = settings.Org;
            string Bucket = settings.Bucket;
            string Table = settings.Table;
            string sensor = settings.sensor;

            string Username = settings.APRSUsername;
            string Passcode = settings.APRSPasscode;
            string URL_APRS = settings.APRSURL;
            int Port = settings.APRSPort;
            string Position = settings.Position;
            string Gateway = settings.APRSGateway;

            TuyaAPI tuyapi = new TuyaAPI(Client_ID, Client_Secret, BaseURL);
            InfluxDBAPI influxdbapi = new InfluxDBAPI(URL_InfluxDB, API_Key, Bucket, Org);
            APRS_API aprsapi = new APRS_API(URL_APRS, Port, Username, Passcode, Software, Position, Gateway);
            MQTTClient mqqtClient = new MQTTClient(null, settings);

            var info = tuyapi.getDeviceInformation(deviceId);
            var deviceName = info.result.name;

            int counter = 10;

            while (true)
            {
                counter++;
                try
                {
                    APRSData aprsData = new APRSData();
                    MQTTData mqttData = new MQTTData();
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
                        //dataline = dataline + dspropertie.code + "=" + dspropertie.value + ",";
                    }
                    dataline = dataline.Remove(dataline.Length - 1, 1);
                    influxdbapi.WriteCurrentData(Table, sensor, dataline, ds.t.ToUnixTimeMilliseconds().ToString());
                    if (counter >= 10)
                    {
                        string LastStep = "";
                        try
                        {
                            info = tuyapi.getDeviceInformation(deviceId);
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
                            Console.WriteLine(ex.Message + " \n " + ex.Source + " \n " + ex.StackTrace + " \n LastStep: " + LastStep, EventLogEntryType.Error, 103);
                        }
                        aprsapi.SendWeatherData(ds.t, aprsData.wind_direct, aprsData.wind_speed_avg, aprsData.wind_speed_gust, aprsData.temp, aprsData.rain, aprsData.humidity, aprsData.pressure);
                        mqqtClient.PushData(mqttData);
                        counter = 0;
                    }
                    System.Threading.Thread.Sleep(30000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message + " - " + ex.Source);
                }
            }
        }
    }
}
