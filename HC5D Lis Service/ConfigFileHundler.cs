using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;

namespace HC5D_Lis_Service
{
     class ConfigFileHundler
    {
        private static object GetAppSetting(string PropertyName, object DefaultValue, Dictionary<string, object> SettingsDictionary)
        {
            if (!SettingsDictionary.ContainsKey(PropertyName))
                return DefaultValue;

            object temp = SettingsDictionary[PropertyName];
            if (temp != null && temp is System.Text.Json.JsonElement)
            {
                System.Text.Json.JsonElement e = (System.Text.Json.JsonElement)temp;
                switch (e.ValueKind)
                {
                    case System.Text.Json.JsonValueKind.String:
                        return e.ToString();
                    case System.Text.Json.JsonValueKind.Number:
                        return e.GetDouble();
                    case System.Text.Json.JsonValueKind.Null:
                        return null;
                    case System.Text.Json.JsonValueKind.False:
                        return false;
                    case System.Text.Json.JsonValueKind.True:
                        return true;
                        //default:
                        //throw new InvalidDataException(e.ValueKind.ToString() + " type not supported in appsettings.json");
                }
            }

            return SettingsDictionary[PropertyName];
        }

        public  IPEndPoint GetEndPoint()
        {
            string AppSettingsFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
            string AppSettingsText = File.ReadAllText(AppSettingsFile);
            Dictionary<string, object> AppSettings = null;
            AppSettings = new Dictionary<string, object>(
               System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(
               AppSettingsText), StringComparer.CurrentCultureIgnoreCase);


            // actual setting configs here:
            string ip_address = (string)GetAppSetting("ip_address", null, AppSettings);
            string port = (string)GetAppSetting("port", null, AppSettings);
            
            Console.WriteLine(ip_address);
            Console.WriteLine(port);
            

            System.Net.IPAddress address = IPAddress.Parse(ip_address);
            System.Net.IPEndPoint endPoint = new IPEndPoint(address, int.Parse(port));

            return endPoint;
            //    private static readonly byte[] ArshoIP = { ip1, ip2, 2, 28 };
            //private static readonly byte[] Localhost = { 127, 0, 0, 1 };

            //string Setting2 = (string)GetAppSetting("MySetting1", "blah", AppSettings);
        }
        public string getPathToLogs()
        {
            string AppSettingsFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
            string AppSettingsText = File.ReadAllText(AppSettingsFile);
            Dictionary<string, object> AppSettings = null;
            AppSettings = new Dictionary<string, object>(
               System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(
               AppSettingsText), StringComparer.CurrentCultureIgnoreCase);


            // actual setting configs here:
           string path_to_logs = (string)GetAppSetting("pathToLogFiles", null, AppSettings);

            return path_to_logs;
        }
        public  string GetCmsApi()
        {
            string AppSettingsFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
            string AppSettingsText = File.ReadAllText(AppSettingsFile);
            Dictionary<string, object> AppSettings = null;
            AppSettings = new Dictionary<string, object>(
               System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(
               AppSettingsText), StringComparer.CurrentCultureIgnoreCase);


            // actual setting configs here:
            string ip_address = (string)GetAppSetting("ip_address", null, AppSettings);
            string port = (string)GetAppSetting("port", null, AppSettings);
            string orbit_cms_api = (string)GetAppSetting("orbit_cms_api", null, AppSettings);
            
            return orbit_cms_api;
            //    private static readonly byte[] ArshoIP = { ip1, ip2, 2, 28 };
            //private static readonly byte[] Localhost = { 127, 0, 0, 1 };

            //string Setting2 = (string)GetAppSetting("MySetting1", "blah", AppSettings);
        }
    }
}
