
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using NostalgiaTV.Hubs;
using NostalgiaTV.Models;
using NostalgiaTV.Models.Configuration;
using Serilog;
using System.Text.Json;
using System.Threading;
using System.Threading.Channels;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NostalgiaTV.Services
{
    public class EpisodeService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private PeriodicTimer _timer;
        private DateTime _lastUpdate = DateTime.UtcNow;

        public EpisodeService(
        IServiceProvider services,
        IOptionsMonitor<AppConfiguration> configuration)
        {
            _services = services;
            _timer = new(TimeSpan.FromMilliseconds(configuration.CurrentValue.UpdateElapsedTime));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Log.Information("EpisodeService started");
            try
            {
                while(await _timer.WaitForNextTickAsync(stoppingToken))
                {
                    await UpdateElapsedTimeAsync();
                }

            }
            catch (OperationCanceledException)
            {
                Log.Information("EpisodeService stopped");
            }
            catch(Exception ex)
            {
                Log.Error(ex, "Error in EpisodeService");
            }
        }

        private async Task UpdateElapsedTimeAsync()
        {
            using var scope = _services.CreateScope();
            var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<ChannelsHub>>();

            try
            {
                var updateInterval = DateTime.UtcNow - _lastUpdate;
                _lastUpdate = DateTime.UtcNow;

                foreach(var channelEntry in ChannelsHub.GetAllChannels())
                {
                    try
                    {
                        var (channelName, currentChannel) = channelEntry;
                        var elapseTime = DateTime.UtcNow - currentChannel.Episode.DatePlay;
                        var newElapsed = (int)elapseTime.TotalSeconds;

                        if (newElapsed != currentChannel.ElapsedTime)
                        {
                            var updateChannel = currentChannel with { ElapsedTime = newElapsed };
                            ChannelsHub.AddOrUpdateChannel(updateChannel);

                            await hubContext.Clients.Group(channelName).SendAsync("ElapsedTime", updateChannel.ElapsedTime);

                            Log.Debug($"Elapsed time updated for channel {channelName}. New Elapsed Time: {newElapsed}");

                            if (newElapsed >= updateChannel.Episode.Duration.TotalSeconds)
                            {
                                Log.Information($"Episode finished for channel {channelName}");

                                await hubContext.Clients.Group(channelName).SendAsync("EpisodeFinished");
                            }
                        }
                    } catch (Exception ex)
                    {
                        Log.Error(ex, $"Error updating elapsed time for channel {channelEntry.Key}");
                    }
                }

            }
            catch(Exception ex)
            {
                Log.Error(ex, "Error updating elapsed time");
            }
        }
    }
}
