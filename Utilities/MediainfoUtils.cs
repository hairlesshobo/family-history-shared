using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using FoxHollow.FHM.Shared.Classes;
using FoxHollow.FHM.Shared.Models;
using FoxHollow.FHM.Shared.Models.Video;

namespace FoxHollow.FHM.Shared.Utilities
{
    // TODO: convert to service
    public static class MediainfoUtils
    {
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
        public static async Task<JsonNode> GetMediainfoAsync(ILogger logger, string filePath, bool useSidecar = true, bool refreshSidecar = false)
        {
            if (logger == null)
                logger = NullLogger.Instance;

            if (!File.Exists(filePath))
                throw new FileNotFoundException(filePath);

            // attempt to load an existing mediainfo sidecar, if present
            string sidecarPath = $"{filePath}.mediainfo";

            // Sidecar exists and we are instructed to use it
            if (useSidecar && !refreshSidecar && File.Exists(sidecarPath))
            {
                logger.LogDebug("Using existing raw mediainfo sidecar");

                using (var sidecarFileHandle = File.Open(sidecarPath, FileMode.Open, FileAccess.Read))
                    return await JsonSerializer.DeserializeAsync<JsonNode>(sidecarFileHandle);
            }
            else
            {
                logger.LogDebug("Calling mediainfo");

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
                        Console.WriteLine("Writing new raw mediainfo sidecar");
                        await File.WriteAllTextAsync(sidecarPath, mediainfo.ToJsonString(Static.DefaultJso));
                    }

                    await process.WaitForExitAsync();

                    return mediainfo;
                }
            }
        }

        /// <summary>
        ///     Generates the model that is to be stored in the sidecar file
        ///     of any raw family video
        /// </summary>
        /// <param name="filePath">Full path to the file</param>
        /// <param name="ctk">Optional cancellation token used to cancel the in-progress generation</param>
        /// <returns>Model used to generate raw sidecar file</returns>
        public static async Task<RawSidecar> GenerateRawSidecarAsync(ILogger logger, string filePath, CancellationToken ctk = default)
        {
            if (logger == null)
                logger = NullLogger.Instance;

            if (ctk == default(CancellationToken))
                ctk = CancellationToken.None;

            if (!File.Exists(filePath))
                throw new FileNotFoundException(filePath);

            var fileInfo = new FileInfo(filePath);
            var mediainfo = await MediainfoUtils.GetMediainfoAsync(logger, filePath);

            var tracks = mediainfo["media"]?["track"];

            if (!tracks.GetType().Equals(typeof(JsonArray)))
            {
                // TODO: what to do if no tracks can be found?
            }

            JsonObject generalTrack = MediainfoUtils.FindTrack((JsonArray)tracks, "general");
            JsonObject videoTrack = MediainfoUtils.FindTrack((JsonArray)tracks, "video");
            JsonObject audioTrack = MediainfoUtils.FindTrack((JsonArray)tracks, "audio");

            var hash = new RawSidecarHash();
            
            using (var fileStream = File.Open(fileInfo.FullName, FileMode.Open, FileAccess.Read))
            {
                var hashGenerator = new HashStreamGenerator(fileStream);
                hashGenerator.GenerateSha1 = true;

                var hashes = await hashGenerator.GenerateAsync(ctk);

                hash.MD5 = hashes.Md5Hash;
                hash.SHA1 = hashes.Sha1Hash;
            }

            var general = new RawSidecarGeneral()
            {
                FileName = fileInfo.Name,
                ContainerFormat = generalTrack["Format"].GetValue<string>(),
                Size = fileInfo.Length,
                Duration = TimeSpan.FromSeconds(Double.Parse(generalTrack["Duration"].GetValue<string>()))
            };

            var encodedDate = generalTrack["Encoded_Date"]?.GetValue<string>();
            if (!String.IsNullOrWhiteSpace(encodedDate))
                general.CaptureDtm = DateTime.ParseExact(encodedDate, "UTC yyyy-MM-dd HH:mm:ss", null);

            var video = new RawSidecarVideo()
            {
                VideoWidth = uint.Parse(videoTrack["Width"].GetValue<string>()),
                VideoHeight = uint.Parse(videoTrack["Height"].GetValue<string>()),
                Format = GetVideoFormat(videoTrack, videoTrack["Format"].GetValue<string>()),
                FormatName = videoTrack["Format_Commercial_IfAny"]?.GetValue<string>(),
                BitrateMode = GetBitrateMode(videoTrack["Bitrate_Mode"]?.GetValue<string>()),
                Bitrate = uint.Parse(videoTrack["BitRate"].GetValue<string>()),
                FramerateMode = GetFramerateMode(videoTrack["FrameRate_Mode"]?.GetValue<string>()), // TODO: It appears that mpeg2 can be assumed to be constant
                FrameRate = Double.Parse(videoTrack["FrameRate"].GetValue<string>()),
                FrameCount = ulong.Parse(videoTrack["FrameCount"].GetValue<string>()),
                ScanType = GetScanType(videoTrack["ScanType"]?.GetValue<string>())
            };
            
            var audio = new RawSidecarAudio()
            {
                Format = audioTrack["Format"].GetValue<string>(),
                BitrateMode = GetBitrateMode(audioTrack["BitRate_Mode"].GetValue<string>()),
                Bitrate = uint.Parse(audioTrack["BitRate"].GetValue<string>()),
                SamplingRate = uint.Parse(audioTrack["SamplingRate"].GetValue<string>()),
                Channels = byte.Parse(audioTrack["Channels"].GetValue<string>()),
                BitDepth = byte.Parse(audioTrack["BitDepth"]?.GetValue<string>() ?? "16")
            };

            // build sidecar model
            var sidecar = new RawSidecar()
            {
                General = general,
                Video = video,
                Audio = audio,
                Hash = hash
            };

            return sidecar;
        }

        private static bool TrackParamEquals(JsonNode node, string strCompare)
            => String.Equals(node.GetValue<string>(), strCompare, StringComparison.InvariantCultureIgnoreCase);

        // TODO: create GetAudioFormat method too

        private static string GetVideoFormat(JsonObject videoTrack, string fallback)
        {
            // TODO: add translation here for mpeg2, h.264, etc

            if (TrackParamEquals(videoTrack["Format"], "MPEG Video") && TrackParamEquals(videoTrack["Format_Version"], "2"))
                return "mpeg2";

            return null;
        }

        private static BitrateMode GetBitrateMode(string input)
        {
            if (String.Equals(input, "CBR", StringComparison.InvariantCultureIgnoreCase))
                return BitrateMode.Constant;
            else if (String.Equals(input, "VBR", StringComparison.InvariantCultureIgnoreCase))
                return BitrateMode.Variable;
            else
                return BitrateMode.Unknown;
        }

        private static FramerateMode GetFramerateMode(string input)
        {
            if (String.Equals(input, "CFR", StringComparison.InvariantCultureIgnoreCase))
                return FramerateMode.Constant;
            else if (String.Equals(input, "VFR", StringComparison.InvariantCultureIgnoreCase))
                return FramerateMode.Variable;
            else
                return FramerateMode.Unknown;
        }

        private static ScanType GetScanType(string input)
        {
            if (String.Equals(input, "Progressive", StringComparison.InvariantCultureIgnoreCase))
                return ScanType.Progressive;
            else if (String.Equals(input, "Interlaced", StringComparison.InvariantCultureIgnoreCase))
                return ScanType.Interlaced;
            else
                return ScanType.Unknown;
        }

        private static JsonObject FindTrack(JsonArray tracks, string findType)
        {
            foreach (JsonNode track in tracks)
            {
                string trackType = track["@type"]?.GetValue<string>();

                if (!String.IsNullOrWhiteSpace(trackType) && trackType.Equals(findType, StringComparison.InvariantCultureIgnoreCase))
                    return (JsonObject)track;
            }

            return null;
        }
    }
}