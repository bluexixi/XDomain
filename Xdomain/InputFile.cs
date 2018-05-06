using NLog;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Xdomain
{
    interface IInputFile
    {
        string Path { get; set; }
        string Hash { get; set; }
        byte[] GetBytes();
        void CalculateHash(HashAlgorithm ha);
    }

    class InputFile : IInputFile
    {
        private static Logger _logger = LogManager.GetCurrentClassLogger();
        public string Path { get; set; }
        public string Hash { get; set; }

        public byte[] GetBytes()
        {
            byte[] data = new byte[0]; ;
            try
            {
                data = File.ReadAllBytes(Path);
            }
            catch (FileNotFoundException)
            {
                _logger.Error("The specified file does not exist, please check your file location.");
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                Environment.Exit(0);
            }
            return data;
        }

        public void CalculateHash(HashAlgorithm ha)
        {
            var res = ha.ComputeHash(GetBytes());
            var sb = new StringBuilder();
            //This is to format the byte array to hexdecimal string, 
            for (int i = 0; i < res.Length; i++)
            {
                sb.Append(res[i].ToString("x2"));
            }

            Hash = sb.ToString();
        }
    }
}
