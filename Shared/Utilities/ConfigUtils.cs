using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Archiver.Shared.Models;
using Archiver.Shared.Models.Config;
using Microsoft.Extensions.Configuration;

namespace Archiver.Shared.Utilities
{
    public static partial class ConfigUtils
    {
        public static string GetConfigDirectory()
        {
            // we set the directory to where the exe is
            string dir = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            
            string configDir = Path.Join(dir, "config/");

            // lets check the current dir and the parent dir for a config directory

            if (Directory.Exists(configDir))
                dir = configDir;
            else
            {
                configDir = Path.Join(dir, "../", "config/");

                if (Directory.Exists(configDir))
                    dir = configDir;
            }

            return dir;
        }

        public static IConfiguration ReadConfigFile()
        {
            string configDir = GetConfigDirectory();

            // we set the directory to where the exe is
            Directory.SetCurrentDirectory(configDir);
            
            IConfiguration _config = new ConfigurationBuilder()
                .SetBasePath(configDir)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .Build();

            return _config;
        }

        public static ArchiverConfig ReadConfig(out List<ValidationError> validationErrors)
        {
            IConfiguration _config = ReadConfigFile();

            ArchiverConfig config = new ArchiverConfig();
            _config.Bind(config);

            validationErrors = config.Validate();
            
            return config;
        }

        public static string BuildValidationPrefix(params string[] values)
            => String.Join('.', values.Where(x => !String.IsNullOrWhiteSpace(x)));
    }
}