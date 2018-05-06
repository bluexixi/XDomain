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
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("apikey", _apikey);
                try
                {
                    var response = await client.PostAsync(_fileUrl, new StreamContent(new MemoryStream(file.GetBytes())));
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
                        var formattedResult = FormatResult(responseJson, fileName);
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

        public static string GetDataId(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                _logger.Error("The input Json string is empty.");
                return string.Empty;
            }
            string dataId = string.Empty;
            try
            {
                var obj = JObject.Parse(json);
                dataId = (string)obj["data_id"];
            }
            catch (JsonReaderException)
            {
                _logger.Error("Incorrect JSON format.");
                Environment.Exit(0);
            }
            return dataId;
        }

        public static string FormatResult(string json, string fileName)
        {
            if (string.IsNullOrEmpty(json))
            {
                _logger.Error("The input Json string is empty.");
                Environment.Exit(0);
            }
            JObject obj = null;
            try
            {
                obj = JObject.Parse(json);
            }
            catch (JsonReaderException)
            {
                _logger.Error("Incorrect JSON format.");
                Environment.Exit(0);
            }
            var results = obj["scan_results"];
            if (results == null)
            {
                //The hash does not exist.
                _logger.Debug("The scan results do not exist.");
                return string.Empty; 
            }
            //Format the response Json to the required output. 
            var sb = new StringBuilder();
            sb.Append(Environment.NewLine);
            sb.Append($"filename: {fileName}");
            sb.Append(Environment.NewLine);
            sb.Append($"overall_status: {(string)results["scan_all_result_a"]}");
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);

            foreach (var pair in (JObject)results["scan_details"])
            {
                sb.Append($"engine: {pair.Key}");
                sb.Append(Environment.NewLine);
                var val = (JObject)pair.Value;
                var scan_result = (string)val["threat_found"];
                scan_result = string.IsNullOrEmpty(scan_result) ? "Clean" : scan_result;
                sb.Append($"threat_found: {scan_result}");
                sb.Append(Environment.NewLine);
                sb.Append($"scan_result: {(string)val["scan_result_i"]}");
                sb.Append(Environment.NewLine);
                sb.Append($"def_time: {(string)val["def_time"]}");
                sb.Append(Environment.NewLine);
                sb.Append(Environment.NewLine);
            }

            sb.Append("END");
            sb.Append(Environment.NewLine);
            return sb.ToString();
        }
    }
}
