using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TuyaAPI.Models;


namespace TuyaAPI
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Weather Tuya API";

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

            TuyaAPI tuyapi = new TuyaAPI(Client_ID, Client_Secret, BaseURL);
            InfluxDBAPI influxdbapi = new InfluxDBAPI(URL_InfluxDB, API_Key, Bucket, Org);
            APRS_API aprsapi = new APRS_API(URL_APRS, Port, Username, Passcode, Software, Position, Gateway);
            int counter = 10;

            while (true)
            {
                counter++;
                try
                {
                    APRSData aprsData = new APRSData();
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
                        aprsapi.SendWeatherData(ds.t, aprsData.wind_direct, aprsData.wind_speed_avg, aprsData.wind_speed_gust, aprsData.temp, aprsData.rain, aprsData.humidity, aprsData.pressure);
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
