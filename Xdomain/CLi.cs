using NLog;
using System;
using System.Configuration;
using System.IO;
using System.Security.Cryptography;
using System.Threading;

namespace Xdomain
{
    class CLi
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        private static readonly int _pollFrequency = int.Parse(ConfigurationManager.AppSettings["pollIntervalInMs"]);
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                _logger.Error("The input command should be 'upload_file file_path'.");
                Environment.Exit(0);
            }
            else if (!string.Equals(args[0], "upload_file", StringComparison.OrdinalIgnoreCase))
            {
                _logger.Error("The input command should be 'upload_file file_path'.");
                Environment.Exit(0);
            }

            IInputFile file = new InputFile { Path = args[1] };
            //Other CryptoServiceProvider can be used to get MD5 or SHA256. 
            file.CalculateHash(new SHA1CryptoServiceProvider());

            _logger.Debug("Looking up the hash...");
            //Blocking call here because the result is needed here immediately for subsequent steps.
            var jsonResult = ApiAccessUtil.CheckHashAsync(file).Result;
            var res = ApiAccessUtil.FormatResult(jsonResult, Path.GetFileName(file.Path));
            //The formatted output result is used to determine whether hash lookup is successful.
            //Hash lookup can fail for different reasons, such as http exceptions or the hash does not exist.
            //Here file upload will only happen when the hash does not exist.
            if (!string.IsNullOrEmpty(res))
            {
                _logger.Debug("The hash exists. Scan Result Output:");
                Console.WriteLine(res);
            }
            else
            {
                _logger.Debug("Uploading the file...");
                //Blocking call here to get the data_id.
                var response = ApiAccessUtil.UploadFileAsync(file).Result;
                var dataId = ApiAccessUtil.GetDataId(response);
                _logger.Debug("Upload completed.");
                _logger.Debug("Retrieving the result using the data id...");
                var completed = false;
                while (!completed)
                {
                    //Repeatedly poll on the data_id to retrive complete result.
                    //Long polling should be better.
                    completed = ApiAccessUtil.RetrieveResultAsync(dataId, Path.GetFileName(file.Path)).Result;
                    //Poll frequency can be changed in the App.config file, here 3000ms is used. 
                    Thread.Sleep(_pollFrequency);
                }
            }
        }
    }
}
