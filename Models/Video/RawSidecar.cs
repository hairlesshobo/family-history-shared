using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FoxHollow.FHM.Shared.Utilities;
using FoxHollow.FHM.Shared.Utilities.Serialization;

namespace FoxHollow.FHM.Shared.Models.Video
{
    public class RawSidecar
    {
        private string RawMediaPath { get; set; }
        private string RawSidecarPath => (!String.IsNullOrWhiteSpace(this.RawMediaPath) ? $"{this.RawMediaPath}.yaml" : null);
        private bool NewSidecar { get; set; } = false;

        public uint Version { get; set; } = 1;
        public DateTime GenerationDtm { get; set; } = DateTime.UtcNow;
        public RawSidecarGeneral General { get; set; } = new RawSidecarGeneral();
        public RawSidecarVideo Video { get; set; } = new RawSidecarVideo();
        public RawSidecarAudio Audio { get; set; } = new RawSidecarAudio();
        public RawSidecarHash Hash { get; set; } = new RawSidecarHash();
        public RawSidecarDetails Details { get; set; } = new RawSidecarDetails();

        public static async Task<RawSidecar> LoadOrGenerateAsync(string filePath, bool regenerate = false, CancellationToken ctk = default)
        {
            string rawSidecarPath = $"{filePath}.yaml";

            if (File.Exists(rawSidecarPath) && !regenerate)
            {
                Console.WriteLine("Loading existing raw sidecar");
                return Yaml.LoadFromFile<RawSidecar>(rawSidecarPath);
            }
            else
            {
                Console.WriteLine("Generating and loading new raw sidecar");
                return await FromMediaFileAsync(filePath, ctk);
            }
        }

        public static async Task<RawSidecar> FromMediaFileAsync(string filePath, CancellationToken ctk = default)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"The specified raw media file does not exist: {filePath}");

            var newSidecar = await MediainfoUtils.GenerateRawSidecarAsync(filePath);
            newSidecar.RawMediaPath = filePath;

            // Automatically write the generated sidecar to disc
            newSidecar.WriteSidecar();

            return newSidecar;
        }

        public void WriteToFile(string filePath, bool overwrite = false)
            => Yaml.DumpToFile(this, filePath, overwrite);

        public void WriteSidecar()
        {
            if (String.IsNullOrWhiteSpace(RawMediaPath) || !File.Exists(RawMediaPath))
                throw new FileNotFoundException("Cannot write raw sidecar because the media file does not exist");

            this.WriteToFile(this.RawSidecarPath, true);
        }
    }
}