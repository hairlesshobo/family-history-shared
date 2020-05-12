namespace Archiver.Classes.Tape
{
    public class TapeSource
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string[] SourcePaths { get; set; }
        public string[] ExcludePaths { get; set; }
        public string[] ExcludeFiles { get; set; }
    }
}