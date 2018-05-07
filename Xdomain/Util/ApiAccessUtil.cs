using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Configuration;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Xdomain
{
    static class ApiAccessUtil
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        //Base url can be used here, but for potential changes, base url with paths are provided in the config file.
        private static string _hashLookupUrl = ConfigurationManager.AppSettings["hashLookupUrl"];
        private static string _fileUrl = ConfigurationManager.AppSettings["fileUrl"];
        private static string _apikey = ConfigurationManager.AppSettings["apikey"];

        public static async Task<string> CheckHashAsync(IInputFile file) {
            if (file == null || string.IsNullOrEmpty(file.Hash))
            {
                _logger.Error("The file or the hash value of the file does not exist.");
                Environment.Exit(0);
            }

            string responseJson = string.Empty;
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("apikey", _apikey);
                var url = Path.Combine(_hashLookupUrl, file.Hash);                
                try
                {
                    var response = await client.GetAsync(url);
                    //Exception will be thrown when status codes do not represent success. 
                    response.EnsureSuccessStatusCode();
                    responseJson = await response.Content.ReadAsStringAsync();
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    Environment.Exit(0);
                }
            }
            return responseJson;
        }

        public static async Task<string> UploadFileAsync(IInputFile file)
        {
            if (file == null || string.IsNullOrEmpty(file.Path))
            {
                _logger.Error("The file or the path of the file does not exist.");
                Environment.Exit(0);
            }

            string responseJson = string.Empty;
            try
            {
                using (var client = new HttpClient())
                using (var fileStream = File.Open(file.Path, FileMode.Open, FileAccess.Read))
                {
                    client.DefaultRequestHeaders.Add("apikey", _apikey);
                    var response = await client.PostAsync(_fileUrl, new StreamContent(fileStream));
                    response.EnsureSuccessStatusCode();
                    responseJson = await response.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                Environment.Exit(0);
            }
            return responseJson;
        }

        public static async Task<bool> RetrieveResultAsync(string dataId, string fileName)
        {
            if (string.IsNullOrEmpty(dataId))
            {
                _logger.Error("Invalid data id.");
                Environment.Exit(0);
            }
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("apikey", _apikey);
                var url = Path.Combine(_fileUrl, dataId);
                try
                {
                    var response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var obj = JObject.Parse(responseJson);
                    var results = obj["scan_results"];
                    //The scan can be "in queue" or "in process" and the scan completes when progress_percentage == 100. 
                    if (results != null && (int)results["progress_percentage"] == 100)
                    {
                        var formattedResult = JsonHelper.FormatResult(responseJson, fileName);
                        _logger.Debug("The scan completed. Scan Result Output:");
                        Console.WriteLine(formattedResult);
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                    Environment.Exit(0);
                }
            }
            return false;
        }
    }
}
