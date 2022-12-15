namespace FoxHollow.FHM.Shared.Models
{
    /// <summary>
    ///     Class that describes the system directories
    /// </summary>
    public class SystemDirectories
    {
        /// <summary>
        ///     Full path to the directory of the executable that is currently running
        /// </summary>
        public string Bin { get; internal protected set; }

        /// <summary>
        ///     Full path to the archive index directory
        /// </summary>
        public string Index { get; internal protected set; }

        /// <summary>
        ///     Full path to the archive json index directory
        /// </summary>
        public string JSON { get; internal protected set; }

        /// <summary>
        ///     Full path to the staging directory to use when archiving to disc
        /// </summary>
        public string DiscStaging { get; internal protected set; }

        /// <summary>
        ///     Full path to the directory where ISO files will be created
        /// </summary>
        public string ISO { get; internal protected set; }
    }
}