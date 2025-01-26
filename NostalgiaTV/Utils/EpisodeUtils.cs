using NostalgiaTV.Models;
using NostalgiaTV.Models.Configuration;
using NostalgiaTV.Models.Hub;
using Serilog;
using System.Text.RegularExpressions;

namespace NostalgiaTV.Utils
{
    public static class EpisodeUtils
    {
        private static readonly Random _rnd = new();
        public static EpisodeHub GetRandomEpisode(string wwwrootPath, List<SerieConfiguration> series)
        {
            var response = new EpisodeHub();
            var storagePath = Path.Combine(wwwrootPath, "storage");

            try
            {
                List<string> seriesPaths = series.Select(serieName => Path.Combine(storagePath, serieName.Folder)).ToList();
                List<List<string>> seasonsPaths = DirectoryUtils.GetSeasonsPaths(seriesPaths);
                List<List<List<string>>> episodesPaths = DirectoryUtils.GetEpisodesPaths(seasonsPaths);

                //Get season and episode random
                int serie = _rnd.Next(seriesPaths.Count);
                int season = _rnd.Next(seasonsPaths[serie].Count);
                int episode = _rnd.Next(episodesPaths[serie][season].Count);

                //EpisodePath
                var episodePath = episodesPaths[serie][season][episode];
                var episodeRelativePath = Path.GetRelativePath(wwwrootPath, episodePath);

                //Get episode data
                using var file = TagLib.File.Create(episodePath);

                //Set episode data
                response.Season = ++season;
                response.Episode = ++episode;
                response.Serie = series[serie].Name;
                response.Path = episodeRelativePath;
                response.Title = GetCleanTitle(file.Name);
                response.Duration = GetDuration(episodePath);
                response.DatePlay = DateTime.UtcNow;
            }
            catch(Exception ex)
            {
                Log.Error(ex, "Error GetRandomEpisode");
            }

            return response;
        }

        private static TimeSpan GetDuration(string filePath)
        {
            using var file = TagLib.File.Create(filePath);
            return file.Properties.Duration;
        }

        private static string GetCleanTitle(string filePath)
        {
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            // Eliminar patrones comunes de numeración
            return Regex.Replace(fileName, @"\b\d{1,2}x\d{2}\b", "", RegexOptions.IgnoreCase).Trim();
        }
    }
}
