using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using NostalgiaTV.Hubs;
using NostalgiaTV.Models.Configuration;
using NostalgiaTV.Models.Hub;
using NostalgiaTV.Utils;
using Serilog;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading.Channels;

namespace NostalgiaTV.Services
{
    public class ChannelService : IHostedService
    {
        // Dependencies
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IServiceProvider _services;
        private readonly IOptionsMonitor<List<ChannelConfiguration>> _channelsConfig;

        // Fields
        private readonly ConcurrentDictionary<string, Timer> _timers = new ConcurrentDictionary<string, Timer>();
        private readonly SemaphoreSlim _syncLock = new SemaphoreSlim(1, 1);
        private CancellationTokenSource _cts = new();

        // Constructor
        public ChannelService(IWebHostEnvironment webHostEnvironment,
                              IServiceProvider services,
                              IOptionsMonitor<List<ChannelConfiguration>> channelsConfig)
        {
            _webHostEnvironment = webHostEnvironment;
            _services = services;
            _channelsConfig = channelsConfig;
        }

        /// <summary>
        /// Start ChannelService
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await InitializeChannelsAsync();
            Log.Information("ChannelService started");
        }

        /// <summary>
        /// Initialize channels
        /// </summary>
        /// <returns></returns>
        private async Task InitializeChannelsAsync()
        {
            foreach (var channelConfig in _channelsConfig.CurrentValue)
            {
                await Task.Run(() => ProcessChannelAsync(channelConfig));
            }
        }

        /// <summary>
        /// Update channel with a new episode
        /// </summary>
        /// <param name="channelConfig"></param>
        private async Task ProcessChannelAsync(ChannelConfiguration channelConfig)
        {
            await _syncLock.WaitAsync();
            try
            {
                using var scope = _services.CreateScope();
                var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<ChannelsHub>>();

                var episode = EpisodeUtils.GetRandomEpisode(_webHostEnvironment.WebRootPath, channelConfig.Series);

                if(episode.Duration <= TimeSpan.Zero)
                {
                    Log.Error($"Error getting episode duration for channel {channelConfig.Name}");
                    return;
                }

                var channel = new Models.Hub.ChannelHub
                {
                    ChannelName = channelConfig.Name,
                    Episode = episode,
                    ElapsedTime = 0
                };

                ChannelsHub.AddOrUpdateChannel(channel);
                Log.Information($"Channel {channelConfig.Name} updated with new episode");
                
                var episodeJson = JsonSerializer.Serialize(channel?.Episode);
                await hubContext.Clients.Group(channel.ChannelName).SendAsync("ReceiveEpisode", episodeJson);

                Log.Information($"Sending Episode for channel {channelConfig.Name}. Episode Data: {episodeJson}");

                await ResetTimerAsync(channelConfig, episode.Duration);
            }
            catch (Exception ex)
            {
               Log.Error(ex, $"Error processing channel {channelConfig.Name}");
                await ScheduleRetry(channelConfig);
            }
            finally
            {
                _syncLock.Release();
            }
        }

        /// <summary>
        /// Force refresh episode for channel
        /// </summary>
        /// <param name="channelName"></param>
        /// <returns></returns>
        public async Task ForceRefreshEpisodeAsync(string channelName)
        {
            var config = _channelsConfig.CurrentValue.FirstOrDefault(c => c.Name == channelName);
            if (config != null)
            {
                await ProcessChannelAsync(config);
            }
        }

        /// <summary>
        /// Reset timer for channel
        /// </summary>
        /// <param name="channelConfig"></param>
        /// <param name="interval"></param>
        /// <returns></returns>
        private async Task ResetTimerAsync(ChannelConfiguration channelConfig, TimeSpan interval)
        {
            if(_timers.TryRemove(channelConfig.Name, out var oldTimer))
                await oldTimer.DisposeAsync();

            var newTimer = new Timer(
                async _ => await ProcessChannelAsync(channelConfig),
                null,
                interval,
                Timeout.InfiniteTimeSpan
                );

            _timers.TryAdd(channelConfig.Name, newTimer);
        }

        /// <summary>
        /// Schedule retry for channel
        /// </summary>
        /// <param name="channelConfig"></param>
        /// <returns></returns>
        private async Task ScheduleRetry(ChannelConfiguration channelConfig)
        {
            var retryTimmer = new Timer(
                async _ => await ProcessChannelAsync(channelConfig),
                null,
                TimeSpan.FromMinutes(1),
                Timeout.InfiniteTimeSpan
                );

            if(_timers.TryGetValue(channelConfig.Name, out var currentTimer))
                await currentTimer.DisposeAsync();
            
            _timers.TryAdd(channelConfig.Name, retryTimmer);
        }

        /// <summary>
        /// Stop ChannelService
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _cts.Cancel();
            foreach (var (name, timer) in _timers)
            {
                await timer.DisposeAsync();
                Log.Information($"Timer for channel {name} stopped");
            }
        }

        /// <summary>
        /// Dispose ChannelService
        /// </summary>
        public void Dispose()
        {
            _cts.Dispose();
            _syncLock.Dispose();
        }
    }
}
