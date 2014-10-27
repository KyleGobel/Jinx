namespace Jinx.Models
{
    public class Compile
    {
        public string Source { get; set; } 
        public string Type { get; set; }
    }

    public class CompileAll
    {
        public string SourceModel { get; set; }
        public string DestinationModel { get; set; }
        public string Map { get; set; }
    }
}