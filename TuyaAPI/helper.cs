using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using TuyaAPI.Models;

namespace TuyaAPI
{
    public static class helper
    {
        public static string CreateToken(this string message, string secret)
        {
            secret = secret ?? "";
            var encoding = new System.Text.ASCIIEncoding();
            byte[] keyByte = encoding.GetBytes(secret);
            byte[] messageBytes = encoding.GetBytes(message);
            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
                return ByteArrayToString(hashmessage).ToUpper();
            }
        }
        public static string ByteArrayToString(byte[] ba)
        {
            return BitConverter.ToString(ba).Replace("-", "");
        }

        public static DateTimeOffset toDateTime(this int unixTime)
        {
            return DateTimeOffset.FromUnixTimeSeconds(unixTime);
        }

        public static string GetSHA256Hash(string content)
        {
            using (var sha = new System.Security.Cryptography.SHA256Managed())
            {
                // Convert the string to a byte array first, to be processed
                byte[] textBytes = System.Text.Encoding.UTF8.GetBytes(content);
                byte[] hashBytes = sha.ComputeHash(textBytes);

                // Convert back to a string, removing the '-' that BitConverter adds
                string hash = BitConverter
                    .ToString(hashBytes)
                    .Replace("-", String.Empty);
                return hash;
            }
        }

        public static float GetWindDirection(string direction)
        {
            switch (direction)
            {
                case "north":
                    return 0;
                case "north_north_east":
                    return 22.5f;
                case "north_east":
                    return 45;
                case "east_north_east":
                    return 67.5f;
                case "east":
                    return 90;
                case "east_south_east":
                    return 112.5f;
                case "south_east":
                    return 135;
                case "south_south_east":
                    return 157.5f;
                case "south":
                    return 180;
                case "south_south_west":
                    return 202.5f;
                case "south_west":
                    return 225;
                case "west_south_west":
                    return 247.5f;
                case "west":
                    return 270;
                case "west_north_west":
                    return 292.5f;
                case "north_west":
                    return 315;
                case "north_north_west":
                    return 337.5f;
                default:
                    return 0;
            }
        }

        public static APRSData GetAPRSData(APRSData data, DeviceStatusModel.Result result)
        {
            switch (result.code)
            {
                case "temp_current_external":
                    data.temp = Convert.ToInt16(result.value);
                    return data;
                case "humidity_outdoor":
                    data.humidity = Convert.ToInt16(result.value);
                    return data;
                case "wind_direct_degree":
                    data.wind_direct = Convert.ToInt16(result.value);
                    return data;
                case "windspeed_avg":
                    data.wind_speed_avg = Convert.ToInt16(result.value);
                    return data;
                case "windspeed_gust":
                    data.wind_speed_gust = Convert.ToInt16(result.value);
                    return data;
                case "rain_24h":
                    data.rain = Convert.ToInt16(result.value);
                    return data;
                case "atmospheric_pressture":
                    data.pressure = Convert.ToInt16(result.value);
                    return data;
                default:
                    return data;
            }
        }

        public static MQTTData GetMQTTData(MQTTData data, DeviceStatusModel.Result result)
        {
            switch (result.code)
            {
                case "temp_current":
                    data.indoor_temp = Convert.ToSingle(result.value) / 10;
                    return data;
                case "humidity_value":
                    data.indoor_humidity = Convert.ToInt16(result.value);
                    return data;
                case "temp_current_external":
                    data.outdoor_temp = Convert.ToSingle(result.value) / 10;
                    return data;
                case "humidity_outdoor":
                    data.outdoor_humidity = Convert.ToInt16(result.value);
                    return data;
                case "wind_direct_degree":
                    data.wind_direct = Convert.ToInt16(result.value);
                    return data;
                case "windspeed_avg":
                    data.wind_speed_avg = Convert.ToInt16(result.value);
                    return data;
                case "rain_24h":
                    data.rain = Convert.ToSingle(result.value) / 1000;
                    return data;
                case "atmospheric_pressture":
                    data.pressure = Convert.ToInt16(result.value);
                    return data;
                case "dew_point_temp":
                    data.dew_point = Convert.ToSingle(result.value) / 10;
                    return data;
                case "bright_value":
                    data.bright = Convert.ToInt32(result.value);
                    return data;
                default:
                    return data;
            }
        }
    }
}
