using System;
using System.IO;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using FoxHollow.FHM.Shared.Classes;
using FoxHollow.FHM.Shared.Models;
using FoxHollow.FHM.Shared.Models.Video;
using FoxHollow.FHM.Shared.Utilities.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace FoxHollow.FHM.Shared.Utilities
{
    public class RawVideoUtils
    {
        private IServiceProvider _services;
        private ILogger _logger;

        public RawVideoUtils(IServiceProvider services)
        {
            _services = services;
            _logger = services.GetRequiredService<ILogger<RawVideoUtils>>();
        }

        public async Task<RawSidecar> LoadOrGenerateAsync(string filePath, bool regenerate = false, CancellationToken ctk = default)
        {
            string rawSidecarPath = $"{filePath}.yaml";

            if (File.Exists(rawSidecarPath) && !regenerate)
            {
                _logger.LogDebug("Loading existing raw sidecar");
                return Yaml.LoadFromFile<RawSidecar>(rawSidecarPath);
            }
            else
            {
                _logger.LogDebug("Generating and loading new raw sidecar");
                return await FromMediaFileAsync(filePath, ctk);
            }
        }

        public async Task<RawSidecar> FromMediaFileAsync(string filePath, CancellationToken ctk = default)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"The specified raw media file does not exist: {filePath}");

            var newSidecar = await this.GenerateRawSidecarAsync(filePath);
            newSidecar.RawMediaPath = filePath;

            // Automatically write the generated sidecar to disc
            newSidecar.WriteSidecar();

            return newSidecar;
        }

                /// <summary>
        ///     Generates the model that is to be stored in the sidecar file
        ///     of any raw family video
        /// </summary>
        /// <param name="filePath">Full path to the file</param>
        /// <param name="ctk">Optional cancellation token used to cancel the in-progress generation</param>
        /// <returns>Model used to generate raw sidecar file</returns>
        public async Task<RawSidecar> GenerateRawSidecarAsync(string filePath, CancellationToken ctk = default)
        {
            var mediainfoUtils = _services.GetRequiredService<MediainfoUtils>();
            
            if (ctk == default(CancellationToken))
                ctk = CancellationToken.None;

            if (!File.Exists(filePath))
                throw new FileNotFoundException(filePath);

            var fileInfo = new FileInfo(filePath);
            var mediainfo = await mediainfoUtils.GetMediainfoAsync(filePath);

            var tracks = mediainfo["media"]?["track"];

            if (!tracks.GetType().Equals(typeof(JsonArray)))
            {
                // TODO: what to do if no tracks can be found?
            }

            JsonObject generalTrack = FindTrack((JsonArray)tracks, "general");
            JsonObject videoTrack = FindTrack((JsonArray)tracks, "video");
            JsonObject audioTrack = FindTrack((JsonArray)tracks, "audio");

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
                FileModifyDtm = fileInfo.LastWriteTimeUtc,
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

        private bool TrackParamEquals(JsonNode node, string strCompare)
            => String.Equals(node.GetValue<string>(), strCompare, StringComparison.InvariantCultureIgnoreCase);

        // TODO: create GetAudioFormat method too

        private string GetVideoFormat(JsonObject videoTrack, string fallback)
        {
            // TODO: add translation here for mpeg2, h.264, etc

            if (TrackParamEquals(videoTrack["Format"], "MPEG Video") && TrackParamEquals(videoTrack["Format_Version"], "2"))
                return "mpeg2";

            return null;
        }

        private BitrateMode GetBitrateMode(string input)
        {
            if (String.Equals(input, "CBR", StringComparison.InvariantCultureIgnoreCase))
                return BitrateMode.Constant;
            else if (String.Equals(input, "VBR", StringComparison.InvariantCultureIgnoreCase))
                return BitrateMode.Variable;
            else
                return BitrateMode.Unknown;
        }

        private FramerateMode GetFramerateMode(string input)
        {
            if (String.Equals(input, "CFR", StringComparison.InvariantCultureIgnoreCase))
                return FramerateMode.Constant;
            else if (String.Equals(input, "VFR", StringComparison.InvariantCultureIgnoreCase))
                return FramerateMode.Variable;
            else
                return FramerateMode.Unknown;
        }

        private ScanType GetScanType(string input)
        {
            if (String.Equals(input, "Progressive", StringComparison.InvariantCultureIgnoreCase))
                return ScanType.Progressive;
            else if (String.Equals(input, "Interlaced", StringComparison.InvariantCultureIgnoreCase))
                return ScanType.Interlaced;
            else
                return ScanType.Unknown;
        }

        private JsonObject FindTrack(JsonArray tracks, string findType)
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