using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DL91
{
    public class ConfigurationHelper
    {
        public static IConfiguration _configuration;
        public static IConfiguration configuration 
        { 
            get
            {
                if(_configuration== null)
                {
                    _configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("config/appsettings.json").Build();
                }
                return _configuration;
            }
        }

        public static string LoginKey => configuration["App:key"];
    }
}
