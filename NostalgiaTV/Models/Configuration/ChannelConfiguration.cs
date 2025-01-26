namespace NostalgiaTV.Models.Configuration
{
    public class ChannelConfiguration
    {
        public string Name { get; set; } = string.Empty;
        public string WebPath { get; set; } = string.Empty;
        public string Logo { get; set; } = string.Empty;
        public string Background { get; set; } = string.Empty;
        public List<SerieConfiguration> Series { get; set; } = new List<SerieConfiguration>();
    }

    public class SerieConfiguration
    {
        public string Name { get; set; } = string.Empty;
        public string Folder { get; set; } = string.Empty;
    }
}
