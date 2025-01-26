namespace NostalgiaTV.Utils
{
    public static class DirectoryUtils
    {
        /// <summary>
        /// Get paths season of all series list
        /// </summary>
        /// <param name="seriesPaths"></param>
        /// <returns></returns>
        public static List<List<string>> GetSeasonsPaths(List<string> seriesPaths)
        {
            return seriesPaths
                .Select(seriesPath => Directory.GetDirectories(seriesPath).ToList())
                .ToList();
        }

        /// <summary>
        /// Gets paths episodes of all season list
        /// </summary>
        /// <param name="seasonsDirectories"></param>
        /// <returns></returns>
        public static List<List<List<string>>> GetEpisodesPaths(List<List<string>> seasonsDirectories)
        {
            return seasonsDirectories
                     .Select(seasonsDirectory =>
                         seasonsDirectory.Select(path =>
                             Directory.GetFiles(path)
                                 .Where(file => file.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase))
                                 .ToList())
                         .ToList())
                     .ToList();
        }
    }

}
