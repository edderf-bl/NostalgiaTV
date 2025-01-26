namespace NostalgiaTV.Models.Hub
{
    public class EpisodeHub
    {
        public int Season { get; set; }
        public int Episode { get; set; }
        public string Serie { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; }
        public DateTime DatePlay { get; set; }
    }
}
