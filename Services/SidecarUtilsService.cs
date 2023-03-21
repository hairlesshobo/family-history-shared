using System;
using System.IO;
using FoxHollow.FHM.Shared.Utilities.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FoxHollow.FHM.Shared.Services;

public class SidecarUtilsService
{
    private IServiceProvider _services;
    private ILogger _logger;

    public SidecarUtilsService(IServiceProvider services)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
        _logger = _services.GetRequiredService<ILogger<SidecarUtilsService>>();
    }

    public void WriteToFile(object sidecarObject, string filePath, bool overwrite = false)
        => Yaml.DumpToFile(sidecarObject, filePath, overwrite);

    public void WriteSidecar(object sidecarObject, string mediaPath)
    {
        if (String.IsNullOrWhiteSpace(mediaPath) || !File.Exists(mediaPath))
            throw new FileNotFoundException("Cannot write media sidecar because the media file does not exist");

        this.WriteToFile(sidecarObject, this.GetSidecarPath(mediaPath), true);
    }

    public string GetSidecarPath(string photoPath)
        => (!String.IsNullOrWhiteSpace(photoPath) ? $"{photoPath}.yaml" : null);

    public TSidecar FromExisting<TSidecar>(string mediaPath)
    {
        var existingSidecarPath = this.GetSidecarPath(mediaPath);

        if (!File.Exists(existingSidecarPath))
            return default(TSidecar);

        var sidecar = Yaml.LoadFromFile<TSidecar>(existingSidecarPath);
        // sidecar.PhotoPath = mediaPath;

        return sidecar;
    }
}