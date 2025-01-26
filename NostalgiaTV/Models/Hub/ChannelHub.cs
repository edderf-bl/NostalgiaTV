namespace NostalgiaTV.Models.Hub
{
    public record ChannelHub
    {
        public string ChannelName { get; set; } = string.Empty;
        public EpisodeHub Episode { get; set; } = new EpisodeHub();
        public double ElapsedTime { get; set; } = 0;
    }
}
