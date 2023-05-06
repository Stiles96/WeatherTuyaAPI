using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using System.Security.Cryptography;
using TuyaAPI.Models;

namespace TuyaAPI
{
    public class TuyaAPI
    {
        /// <summary>
        /// RestClinet
        /// </summary>
        protected RestClient restClient;
        /// <summary>
        /// ClientID
        /// </summary>
        protected string CLIENT_ID;
        /// <summary>
        /// ClientSecret
        /// </summary>
        protected string CLIENT_SECRET;
        /// <summary>
        /// AuthHeader, Baerrer Token
        /// </summary>
        protected AuthResponse authResponse;

        protected string BASE_URL;
        public string version = "v1.0";
        public TokenModel.Token token;

        /// <summary>
        /// New API
        /// </summary>
        /// <param name="apisettings">DELL API Settings</param>
        /// <param name="BaseURL">Base URL of API</param>
        /// <param name="URL">URL of Request</param>
        public TuyaAPI(string Client_ID, string Client_Secret, string BaseURL)
        {
            CLIENT_ID = Client_ID;
            CLIENT_SECRET = Client_Secret;
            BASE_URL = BaseURL;
        }

        /// <summary>
        /// creates hash based on clientid,useraccessToken and time now
        /// some methods requires userAccessToken
        /// </summary>
        /// <param name="useAccessToken">set true if required</param>
        /// <returns></returns>
        private hash getHash(bool useAccessToken = false, string Method = "GET", string URL = "", string body = "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855")
        {
            var atoken = "";
            if (useAccessToken)
            {
                atoken = token.result.access_token;
            }
            var time = new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds();
            var hash = (CLIENT_ID + atoken + time + Method + "\n" + body + "\n\n" + URL).CreateToken(CLIENT_SECRET);
            //var hash = (client_id + atoken + time).CreateToken(secret);
            return new hash { hashdata = hash, time = time };
        }

        private class hash
        {
            public long time { get; set; }
            public string hashdata { get; set; }
        }

        /// <summary>
        /// sends commands as get,put to tuya server and returns as requested type
        /// </summary>
        /// <typeparam name="T">Type of the return object</typeparam>
        /// <param name="newurl">rest url of command</param>
        /// <param name="useAccessToken">if commands needs userAccessToken on request, set it to true</param>
        /// <param name="command">if it is a post operation and have json body, pass your object directly to here</param>
        /// <returns>returns the type you set when requesting.. all responses based on BaseModel</returns>
        private T getRestResult<T>(string newurl, bool useAccessToken = false, object command = null)
        {
            try
            {
                hash h;
                if (command == null)
                    h = getHash(useAccessToken, "GET", newurl.Replace(BASE_URL, ""));
                else
                {
                    h = getHash(useAccessToken, command == null ? "GET" : "POST", newurl.Replace(BASE_URL, ""), helper.GetSHA256Hash(command.ToString()));
                }
                var client = new RestClient(newurl);
                var request = new RestRequest(command == null ? Method.GET : Method.POST);
                request.AddHeader("client_id", CLIENT_ID);
                if (useAccessToken) request.AddHeader("access_token", token.result.access_token);
                request.AddHeader("sign", h.hashdata);
                request.AddHeader("t", $"{h.time}");
                request.AddHeader("sign_method", "HMAC-SHA256");
                if (command != null) request.AddJsonBody(command);
                IRestResponse<T> response = client.Execute<T>(request);
                return response.Data;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error on getting result:{ex.Message}");
                return default(T);

            }
        }

        /// <summary>
        /// gets new Token, you must set clientid, password,url before this method
        /// </summary>
        /// <returns></returns>
        public TokenModel.Token getToken()
        {
            token = getRestResult<TokenModel.Token>($"{BASE_URL}/{version}/token?grant_type=1");
            if (token.success)
            {
                token.result.ExpireDate = DateTime.Now.AddSeconds(token.result.expire_time);

            }
            return token;
        }

        /// <summary>
        /// refresh access token
        /// </summary>
        /// <returns></returns>
        public TokenModel.Token refreshToken()
        {
            token = getRestResult<TokenModel.Token>($"{BASE_URL}/{version}/token/{token.result.access_token}");
            if (token.success)
            {
                token.result.ExpireDate = DateTime.Now.AddSeconds(token.result.expire_time);

            }
            return token;
        }


        /// <summary>
        /// checks token, if expiring in less then 50 seconds, it refreshes the token
        /// </summary>
        public void checkToken()
        {
            for (int i = 0; i < 1; i++)
            {
                if (token == null || !token.success)
                {
                    getToken();
                }
                if ((token.result.ExpireDate - DateTime.Now).TotalSeconds < 50)
                {
                    refreshToken();
                }
                if (token == null || !token.success)
                {
                    System.Threading.Thread.Sleep(30000);
                    i = 0;
                }
            }
        }

        /// <summary>
        /// gets  information about selected deviceId
        /// </summary>
        /// <param name="deviceId">you can find it from getDeviceList 
        /// or from iot.tuya.com->cloud->projects->"yourproject"->device list->app account
        /// , remember to select country</param>
        /// <returns></returns>
        public DeviceInformationModel.Device getDeviceInformation(string deviceId)
        {
            checkToken();
            string newurl = $"{BASE_URL}/{version}/devices/{deviceId}";
            var q = getRestResult<DeviceInformationModel.Device>(newurl, true);

            return q;
        }

        /// <summary>
        /// if a device have saved status,you can get it with this method
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        public DeviceStatusModel.DeviceStatus getDeviceStatus(string deviceId)
        {
            checkToken();
            string newurl = $"{BASE_URL}/{version}/devices/{deviceId}/status";
            var q = getRestResult<DeviceStatusModel.DeviceStatus>(newurl, true);

            return q;
        }

        /// <summary>
        /// returns the device attached to user uuid
        /// but this uuid is not the uid return from token :)
        /// just get any device information, you will get the uid inside the results
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public DeviceListModel.DeviceList getDeviceList(string uid)
        {
            checkToken();
            string newurl = $"{BASE_URL}/{version}/users/{uid}/devices";
            var q = getRestResult<DeviceListModel.DeviceList>(newurl, true);

            return q;
        }

        /// <summary>
        /// this uuid is not the uid return from token :)
        /// just get any device information, you will get the uid inside the results
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        public UserInfoModel.UserInfo getUserInfo(string uid)
        {
            checkToken();
            string newurl = $"{BASE_URL}/{version}/users/{uid}/infos";
            var q = getRestResult<UserInfoModel.UserInfo>(newurl, true);

            return q;
        }

        /// <summary>
        /// if a device have functions to set, you can get them here
        ///<para> if device is DIY type, it returns nothing. </para><br />
        ///<para>item values are stored as enum, integer, boolean etc.. </para>
        ///<para> when using them, just set any name and value</para>
        /// </summary>
        /// <param name="deviceId"></param>
        /// <returns></returns>
        public DeviceFunctionsModel.DeviceFunctions getDeviceFunctions(string deviceId)
        {
            checkToken();
            string newurl = $"{BASE_URL}/{version}/devices/{deviceId}/functions";
            var q = getRestResult<DeviceFunctionsModel.DeviceFunctions>(newurl, true);
            if (q.success)
            {
                foreach (var item in q.result.functions)
                {
                    switch (item.type.ToUpper())
                    {
                        case "ENUM":
                            item.values = SimpleJson.DeserializeObject<DataTypes.Enum>(item.values.ToString());
                            break;
                        case "INTEGER":
                            item.values = SimpleJson.DeserializeObject<DataTypes.Integer>(item.values.ToString().Replace("\"unit\":}", "\"unit\":\"\"}"));
                            break;
                        case "BOOLEAN":
                            item.values = true;
                            break;
                        default:
                            Console.WriteLine($"item type:{item.type},values:{item.values}");
                            break;
                    }

                }
            }

            return q;
        }

        /// <summary>
        /// returns all function types from tuya ecosystem
        /// </summary>
        /// <returns></returns>
        public FunctionListModel.Functions getFunctionList()
        {
            checkToken();
            string newurl = $"{BASE_URL}/{version}/functions/dj";
            var q = getRestResult<FunctionListModel.Functions>(newurl, true);

            return q;
        }

        /// <summary>
        /// this is raw command operation
        /// </summary>
        /// <param name="deviceId">deviceId of device</param>
        /// <param name="command">command to execute ex:switch</param>
        /// <param name="value">command value,it can be string,boolean,integer ex: true</param>
        /// <returns></returns>
        public BaseModel sendCommand(string deviceId, string command, object value)
        {
            CommandModel.Commands cmd = new CommandModel.Commands();
            cmd.commands = new List<CommandModel.Command>();
            cmd.commands.Add(new CommandModel.Command { code = command, value = value });
            checkToken();
            string newurl = $"{BASE_URL}/{version}/devices/{deviceId}/commands";
            var q = getRestResult<BaseModel>(newurl, true, cmd);

            return q;
        }

        /// <summary>
        /// if you want to send multiple commands in one shot, use this
        /// </summary>
        /// <param name="deviceId">id of device</param>
        /// <param name="commands">its a list of command with values</param>
        /// <returns></returns>
        public BaseModel sendCommands(string deviceId, List<CommandModel.Command> commands)
        {
            CommandModel.Commands cmd = new CommandModel.Commands();
            cmd.commands = commands;
            checkToken();
            string newurl = $"{BASE_URL}/{version}/devices/{deviceId}/commands";
            var q = getRestResult<BaseModel>(newurl, true, cmd);

            return q;
        }

        /// <summary>
        /// if command name switch_1, you can use this method
        /// </summary>
        /// <param name="deviceId">id of device</param>
        /// <returns>returns a model if command executed successfully</returns>
        public BaseModel TurnSwitchOn(string deviceId)
        {
            return sendCommand(deviceId, "switch_1", true);
        }

        /// <summary>
        /// if command name switch_1, you can use this method
        /// </summary>
        /// <param name="deviceId">id of device</param>
        /// <returns>returns a model if command executed successfully</returns>
        public BaseModel TurnSwitchOff(string deviceId)
        {
            return sendCommand(deviceId, "switch_1", false);
        }

        /// <summary>
        /// if command name switch, you can use this method
        /// </summary>
        /// <param name="deviceId">id of device</param>
        /// <returns>returns a model if command executed successfully</returns>
        public BaseModel TurnOn(string deviceId)
        {
            return sendCommand(deviceId, "switch", true);
        }

        /// <summary>
        /// if command name switch, you can use this method
        /// </summary>
        /// <param name="deviceId">id of device</param>
        /// <returns>returns a model if command executed successfully</returns>
        public BaseModel TurnOff(string deviceId)
        {
            return sendCommand(deviceId, "switch", false);
        }

        /// <summary>
        /// sets operation mode of device
        /// actually it sends command "mode" and string value
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="mode">its an enum, but represents "cold","auto","dehumidification","wind_dry","heat" string values</param>
        /// <returns></returns>
        public BaseModel setClimateMode(string deviceId, ClimateCommands.enModes mode)
        {
            return sendCommand(deviceId, "mode", ClimateCommands.getMode(mode));
        }

        /// <summary>
        /// sets operation mode of device
        /// actually it sends command "mode" and string value
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="mode">  represents "cold","auto","dehumidification","wind_dry","heat" etc.. string values</param>
        /// <returns></returns>
        public BaseModel setClimateMode(string deviceId, string mode)
        {
            return sendCommand(deviceId, "mode", mode);
        }


        /// <summary>
        /// sets climate device's temperature
        /// </summary>
        /// <param name="deviceId">id of device</param>
        /// <param name="Temp">integer value between 16,30</param>
        /// <returns></returns>
        public BaseModel setClimateTemperature(string deviceId, int Temp)
        {
            return sendCommand(deviceId, "temp", Temp);
        }

        /// <summary>
        /// sets climate device operation modes
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="mode">its an enum, but represents "mid","low", "auto",  "high" string values</param>
        /// <returns></returns>
        public BaseModel setClimateFan(string deviceId, ClimateCommands.enFanModes mode)
        {
            return sendCommand(deviceId, "fan", ClimateCommands.getFanMode(mode));
        }

        /// <summary>
        /// sets climate device operation modes, it just sends "fan" command with string value
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="mode"> "mid","low", "auto",  "high" string values</param>
        /// <returns></returns>
        public BaseModel setClimateFan(string deviceId, string mode)
        {
            return sendCommand(deviceId, "fan", mode);
        }
    }
}
