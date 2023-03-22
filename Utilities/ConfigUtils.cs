// //==========================================================================
// //  Family History Manager - https://code.foxhollow.cc/fhm/
// //
// //  A cross platform tool to help organize and preserve all types
// //  of family history
// //==========================================================================
// //  Copyright (c) 2020-2023 Steve Cross <flip@foxhollow.cc>
// //
// //  Licensed under the Apache License, Version 2.0 (the "License");
// //  you may not use this file except in compliance with the License.
// //  You may obtain a copy of the License at
// //
// //       http://www.apache.org/licenses/LICENSE-2.0
// //
// //  Unless required by applicable law or agreed to in writing, software
// //  distributed under the License is distributed on an "AS IS" BASIS,
// //  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// //  See the License for the specific language governing permissions and
// //  limitations under the License.
// //==========================================================================

// using System;
// using System.Collections.Generic;
// using System.IO;
// using System.Linq;
// using System.Text;
// using FoxHollow.FHM.Shared.Models;
// using FoxHollow.FHM.Shared.Models.Config;
// using Microsoft.Extensions.Configuration;

// namespace FoxHollow.FHM.Shared.Utilities
// {
//     public static partial class ConfigUtils
//     {
//         public static string GetConfigDirectory()
//         {
//             // we set the directory to where the exe is
//             string dir = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);

//             string configDir = Path.Join(dir, "config/");

//             // lets check the current dir and the parent dir for a config directory

//             if (Directory.Exists(configDir))
//                 dir = configDir;
//             else
//             {
//                 configDir = Path.Join(dir, "../", "config/");

//                 if (Directory.Exists(configDir))
//                     dir = configDir;
//             }

//             return dir;
//         }

//         public static IConfiguration ReadConfigFile()
//         {
//             string configDir = GetConfigDirectory();

//             // we set the directory to where the exe is
//             Directory.SetCurrentDirectory(configDir);

//             IConfiguration _config = new ConfigurationBuilder()
//                 .SetBasePath(configDir)
//                 .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
//                 .Build();

//             return _config;
//         }

//         public static ArchiverConfig ReadConfig(out List<ValidationError> validationErrors)
//         {
//             IConfiguration _config = ReadConfigFile();

//             ArchiverConfig config = new ArchiverConfig();
//             _config.Bind(config);

//             validationErrors = config.Validate();

//             return config;
//         }

//         public static string BuildValidationPrefix(params string[] values)
//             => String.Join('.', values.Where(x => !String.IsNullOrWhiteSpace(x)));
//     }
// }