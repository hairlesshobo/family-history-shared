using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using FoxHollow.FHM.Shared.Models.Video;

namespace FoxHollow.FHM.Shared.Utilities
{
    public static class MediainfoUtils
    {
        public static async Task<JsonNode> GetMediainfoAsync(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException(filePath);

            using(var process = new Process())
            {
                process.StartInfo.FileName = "mediainfo";
                process.StartInfo.ArgumentList.Add("--Output=JSON");
                process.StartInfo.ArgumentList.Add(filePath);
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.Start();

                var output = await JsonSerializer.DeserializeAsync<JsonNode>(process.StandardOutput.BaseStream);

                await process.WaitForExitAsync();

                return output;
            }
        }

        public static async Task<RawSidecar> GenerateRawSidecarAsync(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException(filePath);

            var fileInfo = new FileInfo(filePath);
            var mediainfo = await MediainfoUtils.GetMediainfoAsync(filePath);

            var tracks = mediainfo["media"]?["track"];

            if (!tracks.GetType().Equals(typeof(JsonArray)))
            {
                // TODO: what to do if no tracks can be found?
            }

            JsonObject generalTrack = MediainfoUtils.FindTrack((JsonArray)tracks, "general");
            JsonObject videoTrack = MediainfoUtils.FindTrack((JsonArray)tracks, "video");
            JsonObject audioTrack = MediainfoUtils.FindTrack((JsonArray)tracks, "audio");



            Console.WriteLine(generalTrack.ToJsonString());

            // Console.WriteLine(sidecar.MediaInfo["creatingLibrary"]?["meow"]?.ToJsonString());


            // build sidecar model
            var sidecar = new RawSidecar();
            sidecar.MediaInfo = mediainfo;
            sidecar.General = new RawSidecarGeneral()
            {
                Size = fileInfo.Length,
                FileName = fileInfo.Name,
                Duration = TimeSpan.FromSeconds(Double.Parse(generalTrack["Duration"].GetValue<string>())),
                CaptureDtm = DateTime.ParseExact(generalTrack["Encoded_Date"].GetValue<string>(), "UTC yyyy-MM-dd HH:mm:ss", null),
                ContainerFormat = generalTrack["Format"].GetValue<string>()
            };

            return sidecar;
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