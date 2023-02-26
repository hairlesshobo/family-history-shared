using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FoxHollow.FHM.Shared.Models;
using ImageMagick;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FoxHollow.FHM.Shared.Classes;

public class PhotoProcessor
{
    private IServiceProvider _services;
    private ILogger _logger;

    public string Directory { get; set; }
    public bool Recursive { get; set; }
    public int ThumbnailSize { get; set; } = 150;
    public string ThumbnailExtension { get; set; } = ".jtmb";
    public int PreviewSize { get; set; } = 1200;
    public string PreviewExtension { get; set; } = ".jprv";
    public List<string> IncludePaths { get; set; } = new List<string>();
    public List<string> ExcludePaths { get; set; } = new List<string>();
    public List<string> IncludeExtensions { get; set; } = new List<string>();

    public PhotoProcessor(IServiceProvider provider)
    {
        _services = provider;
        _logger = provider.GetRequiredService<ILogger<PhotoProcessor>>();
    }

    public async Task<ActionQueue> ProcessPhotos(CancellationToken ctk)
    {
        if (String.IsNullOrWhiteSpace(this.Directory))
            throw new ArgumentNullException(nameof(this.Directory));

        // TODO: actually use the cancellation token
        var actionQueue = new ActionQueue();

        var treeWalkerFactory = _services.GetRequiredService<MediaTreeWalkerFactory>();

        var treeWalker = treeWalkerFactory.GetWalker(this.Directory);
        treeWalker.Recursive = this.Recursive;
        treeWalker.IncludePaths = this.IncludePaths;
        treeWalker.ExcludePaths = this.ExcludePaths;
        treeWalker.IncludeExtensions = this.IncludeExtensions;

        await foreach (var collection in treeWalker.StartScanAsync())
        {
            var entries = collection.Entries.Where(x => x.Ignored == false);

            // TODO: build media type priority logic
            if (entries.Count() > 1)
                throw new Exception("More than one non-ignore media file entries, unknown how to proceed!");

            var entry = entries.First();

            // TODO: status update here
            _logger.LogInformation($"{entry.Path}");

            if (new string[] { "tiff", "tif" }.Contains(entry.FileInfo.Extension.ToLower()))
                await ProcessTiffPhoto(entry);
        }

        return actionQueue;
    }

    private async Task ProcessTiffPhoto(MediaFileEntry entry)
    {
        PhotoSidecar sidecar = PhotoSidecar.FromExisting(entry.FileInfo.FullName);
        bool process = false;

        if (sidecar != null)
        {
            _logger.LogInformation($"Reading existing sidecar file");

            // if the known size in the sidecar doesn't match the current file size, force a reprocess
            if (sidecar.General.Size != entry.FileInfo.Length)
            {
                _logger.LogInformation($"Size changed since last scan, reprocessing");
                process = true;
            }

            // If this is a known uncompressed tiff file, for some reason, we force a reprocess
            if (sidecar.Format.Format.Equals("image/tiff") && sidecar.Format.Compression.Equals("NoCompression"))
            {
                _logger.LogInformation($"Found uncompressed tiff, reprocessing");
                process = true;
            }
        }
        else
        {
            _logger.LogInformation($"Generating new sidecar");

            sidecar = new PhotoSidecar(entry.Path);
            process = true;
        }

        if (process)
        {
            using (var mimage = new MagickImage(entry.FileInfo))
            {
                if (mimage.FormatInfo.MimeType != "image/tiff")
                {
                    _logger.LogWarning($"File is not a valid tiff file: {entry.Path}");
                    process = false;
                }

                else
                {
                    string imageMd5 = null;

                    if (mimage.Compression == CompressionMethod.NoCompression)
                    {
                        process = true;
                        var tmpPath = Path.Combine(entry.FileInfo.Directory.FullName, $"{entry.FileInfo.Name}.ltmp");

                        if (File.Exists(tmpPath))
                        {
                            _logger.LogWarning($"Compression tmp path already exists, removing: {tmpPath}");
                            File.Delete(tmpPath);
                        }

                        string uncompressedMd5 = await GenerateImageMd5(mimage);

                        _logger.LogInformation($"Compressing image to temp path: {tmpPath}");
                        mimage.Settings.Compression = CompressionMethod.LZW;
                        mimage.Write(tmpPath);
                        string compressedMd5 = await GenerateImageMd5(mimage);

                        if (String.Equals(compressedMd5, uncompressedMd5))
                        {
                            _logger.LogInformation("Compressed MD5 verification successful, replacing uncompressed image");

                            var bakPath = Path.Combine(entry.FileInfo.Directory.FullName, $"{entry.FileInfo.Name}.bak");

                            // For now, we move the file to a .bak file (in the future, we'll make it configurable to either move or remove)
                            File.Move(entry.FileInfo.FullName, bakPath);

                            // Move the compressed image into place
                            File.Move(tmpPath, entry.FileInfo.FullName);
                            entry.RefreshFileInfo();

                            imageMd5 = compressedMd5;
                        }
                        else
                            _logger.LogWarning("MD5 mismatch between compressed and uncompressed image!");
                    }

                    sidecar.General.Size = entry.FileInfo.Length;
                    sidecar.General.OriginalFileName = entry.FileInfo.Name;
                    sidecar.General.FileModifyDtm = entry.FileInfo.LastWriteTimeUtc;

                    await ProcessImageHashes(sidecar, mimage, process);

                    // if the image md5 hasn't been generated, make it happen
                    if (String.IsNullOrWhiteSpace(sidecar.Hash.ImageMD5))
                        sidecar.Hash.ImageMD5 = imageMd5 ?? await GenerateImageMd5(mimage);

                    sidecar.Format.Width = mimage.Width;
                    sidecar.Format.Height = mimage.Height;
                    sidecar.Format.Colorspace = mimage.ColorSpace.ToString();
                    sidecar.Format.Compression = mimage.Settings.Compression.ToString();
                    sidecar.Format.Format = mimage.FormatInfo.MimeType;
                    sidecar.Format.Pages = 1; // TODO: add support for multipage tiff?

                    sidecar.Previews.GeneratedDtm = DateTime.UtcNow;
                    sidecar.Previews.SourceImageMd5 = sidecar.Hash.ImageMD5;
                    sidecar.Previews.SourceSize = sidecar.General.Size;

                    sidecar.Previews.Thumbnail = GeneratePreviewImage(mimage, this.ThumbnailSize, entry.Path, this.ThumbnailExtension);
                    sidecar.Previews.Preview = GeneratePreviewImage(mimage, this.PreviewSize, entry.Path, this.PreviewExtension);
                }
            }

            _logger.LogInformation("Writing sidecar file");
            sidecar.WriteSidecar();
        }
    }

    private PhotoSidecarPreviewImage GeneratePreviewImage(MagickImage image, int size, string pathNoExtension, string extension)
    {
        var previewImageObj = new PhotoSidecarPreviewImage();
        using (var previewImage = (MagickImage)image.Clone())
        {
            var outputPath = $"{pathNoExtension}{extension}";

            previewImage.Format = MagickFormat.Jpeg;
            previewImage.Resize(size, size);
            previewImage.Write(outputPath);

            previewImageObj.Width = previewImage.Width;
            previewImageObj.Height = previewImage.Height;
            previewImageObj.Format = previewImage.FormatInfo.MimeType;
            previewImageObj.FileExtension = extension;
            previewImageObj.Size = previewImage.ToByteArray().Length;
        }

        return previewImageObj;
    }

    private async Task ProcessImageHashes(PhotoSidecar sidecar, MagickImage mimage, bool forceProcess)
    {
        if (String.IsNullOrWhiteSpace(sidecar.Hash.MD5) || String.IsNullOrWhiteSpace(sidecar.Hash.SHA1) || forceProcess)
        {
            using (var stream = new MemoryStream(mimage.ToByteArray()))
            {
                var hsg = new HashStreamGenerator(stream)
                {
                    GenerateMd5 = true,
                    GenerateSha1 = true
                };
                await hsg.GenerateAsync();

                sidecar.Hash.MD5 = hsg.Md5Hash;
                sidecar.Hash.SHA1 = hsg.Sha1Hash;
            }
        }
    }

    private async Task<string> GenerateImageMd5(MagickImage image, CancellationToken ctk = default)
    {
        using (var memStream = new MemoryStream())
        using (var newImg = (MagickImage)image.Clone())
        {
            // Sets the output format to png
            newImg.Format = MagickFormat.Rgb;

            // Write the image to the memorystream
            newImg.Write(memStream);

            memStream.Seek(0, SeekOrigin.Begin);

            var hsg = new HashStreamGenerator(memStream);
            hsg.GenerateMd5 = true;
            hsg.GenerateSha1 = false;

            await hsg.GenerateAsync(ctk);

            return hsg.Md5Hash;
        }
    }
}