using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using TuyaAPI.Models;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

namespace TuyaAPI
{
    public class APRS_API
    {
        string Username;
        string Passcode;
        string URL;
        int Port;
        string Software;
        string Position;
        string Gateway;

        TcpClient client;
        NetworkStream stream;

        public APRS_API(string URL, int Port, string Username, string Passcode, string Software, string Position, string Gateway, EventLog eventLog = null)
        {
            this.URL = URL;
            this.Port = Port;
            this.Username = Username;
            this.Passcode = Passcode;
            this.Software = Software;
            this.Position = Position;
            this.Gateway = Gateway;
            ConnectAPRS(eventLog);
        }

        string GetBase64String(string text)
        {
            var textBytes = System.Text.Encoding.UTF8.GetBytes(text);
            return System.Convert.ToBase64String(textBytes);
        }

        public void SendWeatherData(DateTimeOffset time, int winddirection, int windspeed, int windspeed_gust, int temperature, int rainday, int humidity, int pressure, EventLog eventLog = null)
        {
            string timez = time.ToString("HHmmss") + "z";
            string speed = (windspeed * 2.237 / 10).ToString("000");
            string gust = (windspeed_gust * 2.237 / 10).ToString("000");
            string temp = (temperature * 1.8 / 10 + 32).ToString("000");
            string rain = (rainday / 25.4 / 10).ToString("000");
            string dataline = Username + ">APRS:@" + timez + Position + "_" + winddirection.ToString("000") + "/" + speed + "g" + gust + "t" + temp + "P" + rain + "h" + humidity.ToString("00") + "b" + pressure.ToString("00000") + " Send by own API: http://weather.he-it.eu";

            Byte[] data;
            data = System.Text.Encoding.ASCII.GetBytes(dataline + "\n");
            if (client.Connected)
                ConnectAPRS(eventLog);
            stream.Write(data, 0, data.Length);
            Console.WriteLine(dataline);
            if (eventLog != null)
                eventLog.WriteEntry(dataline, EventLogEntryType.Information, 21);
            

            //string command = "plink.exe -telnet " + URL + " -P " + Port.ToString() + " -batch < commands.txt";

            //RestRequest request = new RestRequest(Method.POST);
            //request.AddParameter("text/plain", dataline, ParameterType.RequestBody);
            //IRestResponse response = restClient.Execute(request);
        }

        void Read(NetworkStream stream, EventLog eventLog = null)
        {
            Byte[] data = new byte[256];
            String responseData = String.Empty;
            
            Int32 bytes = stream.Read(data, 0, data.Length);
            responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
            Console.WriteLine(responseData);
            if (eventLog != null)
                eventLog.WriteEntry(responseData, EventLogEntryType.Information, 20);
        }

        void ConnectAPRS(EventLog eventLog = null)
        {
            string Login = "user " + Username + " pass " + Passcode + " vers " + Software;
            client = new TcpClient();
            client.Connect(URL, Port);
            Byte[] data;
            stream = client.GetStream();
            Read(stream, eventLog);

            data = System.Text.Encoding.ASCII.GetBytes("\n");
            stream.Write(data, 0, data.Length);

            data = System.Text.Encoding.ASCII.GetBytes(Login + "\n");
            stream.Write(data, 0, data.Length);
            Console.WriteLine(Login);
            System.Threading.Thread.Sleep(2000);
            Read(stream, eventLog);
        }
    }
}
