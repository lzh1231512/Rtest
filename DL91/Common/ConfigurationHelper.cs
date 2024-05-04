using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
                if (_configuration == null)
                {
                    var path = FixPath("config/appsettings.json");
                    _configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile(path).Build();
                }
                return _configuration;
            }
        }

        public static string FixPath(string path)
        {
            if (!File.Exists(path))
            {
                string assemblyLocation = Assembly.GetExecutingAssembly().Location;
                string dllFolder = Path.GetDirectoryName(assemblyLocation);
                path = dllFolder + "/" + path;
            }
            return path;
        }

        public static string LoginKey => configuration["App:key"];

        public static IEnumerable<int> DisabledType => configuration["App:disabledType"].Split(',').Select(f => int.Parse(f));

        public static string dbPath => FixPath(configuration["App:dbPath"]);
    }

}
