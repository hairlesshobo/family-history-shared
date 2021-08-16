using Archiver.Classes.Disc;

namespace Archiver.Utilities.Disc
{
    public static class DiscFormatting
    {
        public static string GetDiscName(DiscDetail disc)
            => $"Disc {disc.DiscNumber.ToString("0000")}";
    }
}