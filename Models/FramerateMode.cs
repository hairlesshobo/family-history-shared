using YamlDotNet.Serialization;
namespace FoxHollow.FHM.Shared.Models
{
    public enum FramerateMode
    {
        [YamlMember()]
        Unknown,
        Constant,
        Variable
    }
}