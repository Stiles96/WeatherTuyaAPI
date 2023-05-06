using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;

namespace TuyaAPI
{
    public class InfluxDBAPI
    {
        string API_KEY;
        string URL;
        string BUCKET;
        string ORG;

        RestClient restClient;

        public InfluxDBAPI(string URL, string API_Key, string Bucket, string Org)
        {
            this.URL = URL;
            this.API_KEY = API_Key;
            this.BUCKET = Bucket;
            this.ORG = Org;
            restClient = new RestClient(URL);
            restClient.AddDefaultHeader("Authorization", "Token " + API_KEY);
            restClient.AddDefaultHeader("Content-Type", "text/plain; charset=utf-8");
            restClient.AddDefaultHeader("Accept", "application/json");
            
        }

        public void WriteCurrentData(string table, string Sensor, string DataLine, string time)
        {
            RestRequest request = new RestRequest("/api/v2/write?org=" + ORG + "&bucket=" + BUCKET + "&precision=ns", Method.POST);
            //request.AddParameter("org", ORG);
            //request.AddParameter("bucket", BUCKET);
            //request.AddParameter("precision", "ns");

            string body = table + ",sensor=" + Sensor + " " + DataLine;
            request.AddParameter("text/plain", body, ParameterType.RequestBody);

            IRestResponse response = restClient.Execute(request);
        }
    }
}
