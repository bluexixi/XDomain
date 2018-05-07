using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Text;

namespace Xdomain
{
    static class JsonHelper
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();

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
