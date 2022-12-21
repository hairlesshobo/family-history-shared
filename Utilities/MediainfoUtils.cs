using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace FoxHollow.FHM.Shared.Utilities
{
    public class MediainfoUtils
    {
        private IServiceProvider _services;
        private ILogger _logger;

        public MediainfoUtils(IServiceProvider services)
        {
            _services = services;

            _logger = _services.GetRequiredService<ILogger<MediainfoUtils>>();
        }

        /// <summary>
        ///     Gets the MediaInfo as a JsonNode object, optionally using and creating
        ///     a `.mediainfo` sidecar, if useSidecar is enabled
        /// </summary>
        /// <param name="filePath">
        ///     Full path to the media file to read the mediainfo for
        /// </param>
        /// <param name="useSidecar">
        ///     If true (default), it will attempt to read a .mediainfo sidecar, if present, and 
        ///     create a sidecar if it does not already exist
        /// </param>
        /// <param name="refreshSidecar">
        ///     If true, the mediainfo sidecar will be refreshed, even if it already exists
        /// </param>
        /// <returns>Task that returns the JsonNode</returns>
        public async Task<JsonNode> GetMediainfoAsync(string filePath, bool useSidecar = true, bool refreshSidecar = false)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException(filePath);

            // attempt to load an existing mediainfo sidecar, if present
            string sidecarPath = $"{filePath}.mediainfo";

            // Sidecar exists and we are instructed to use it
            if (useSidecar && !refreshSidecar && File.Exists(sidecarPath))
            {
                _logger.LogDebug("Using existing raw mediainfo sidecar");

                using (var sidecarFileHandle = File.Open(sidecarPath, FileMode.Open, FileAccess.Read))
                    return await JsonSerializer.DeserializeAsync<JsonNode>(sidecarFileHandle);
            }
            else
            {
                _logger.LogDebug("Calling mediainfo");

                using(var process = new Process())
                {
                    process.StartInfo.FileName = "mediainfo";
                    process.StartInfo.ArgumentList.Add("--Output=JSON");
                    process.StartInfo.ArgumentList.Add(filePath);
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.Start();

                    JsonNode mediainfo = await JsonSerializer.DeserializeAsync<JsonNode>(process.StandardOutput.BaseStream);

                    if (useSidecar)
                    {
                        _logger.LogDebug("Writing new raw mediainfo sidecar");
                        await File.WriteAllTextAsync(sidecarPath, mediainfo.ToJsonString(Static.DefaultJso));
                    }

                    await process.WaitForExitAsync();

                    return mediainfo;
                }
            }
        }
    }
}