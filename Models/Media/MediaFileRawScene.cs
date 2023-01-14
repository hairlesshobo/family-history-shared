using System;
using System.IO;
using System.Linq;
using FoxHollow.FHM.Shared.Models.Video;
using FoxHollow.FHM.Shared.Services;
using FoxHollow.FHM.Shared.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace FoxHollow.FHM.Shared.Models;

public class MediaFileRawScene
{
    public string Path { get; set; }
    public string RootRelativePath { get; set; }
    public DirectoryInfo DirectoryInfo { get; set; }
    public string CameraName { get; set; }
    public CameraProfile CameraProfile { get; set; }

    internal MediaFileRawScene(IServiceProvider services, string rootPath, string path)
    {
        if (!Directory.Exists(path))
            throw new DirectoryNotFoundException(path);

        var camProfileService = services.GetRequiredService<CamProfileService>();

        this.Path = PathUtils.CleanPath(path);
        this.RootRelativePath = PathUtils.GetRootRelativePath(rootPath, path);
        this.DirectoryInfo = new DirectoryInfo(this.Path);
        this.CameraName = this.DirectoryInfo.Name;
        this.CameraProfile = camProfileService.Profiles.FirstOrDefault(x => x.Name == this.CameraName);
    }
}