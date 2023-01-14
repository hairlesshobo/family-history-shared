using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using FoxHollow.FHM.Shared.Models.Video;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FoxHollow.FHM.Shared.Services;

public class CamProfileService
{
    private IServiceProvider _services;
    private ILogger _logger;
    private List<CameraProfile> _profiles;

    public IReadOnlyList<CameraProfile> Profiles => (IReadOnlyList<CameraProfile>)_profiles;

    public IReadOnlyList<string> CamNames => (IReadOnlyList<string>)this.Profiles.Select(x => x.Name);

    public CamProfileService(IServiceProvider services)
    {
        _services = services;
        _logger = _services.GetRequiredService<ILogger<CamProfileService>>();

        _profiles = this.LoadCameraProfiles();
    }

    private List<CameraProfile> LoadCameraProfiles()
    {
        var profiles = new List<CameraProfile>();
        var profileDirPath = Path.Join(SysInfo.ConfigRoot, "profiles");
        var filesPaths = Directory.GetFiles(profileDirPath, "*.json");

        foreach (var filePath in filesPaths)
        {
            using (var fileHandle = File.OpenRead(filePath))
            {
                var camProfile = JsonSerializer.Deserialize<CameraProfile>(fileHandle, Static.DefaultJso);
                profiles.Add(camProfile);
            }
        }

        return profiles;
    }
}