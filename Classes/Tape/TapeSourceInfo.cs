namespace Archiver.Classes.Tape
{
    public class TapeSourceInfo
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string[] SourcePaths { get; set; }
        public string[] ExcludePaths { get; set; }
        public string[] ExcludeFiles { get; set; }
    }
}