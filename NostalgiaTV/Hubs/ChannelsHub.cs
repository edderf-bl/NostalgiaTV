using Microsoft.AspNetCore.SignalR;
using NostalgiaTV.Models.Hub;
using Serilog;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading.Channels;

namespace NostalgiaTV.Hubs
{
    public class ChannelsHub : Hub
    {
        private static readonly ConcurrentDictionary<string, ChannelHub> _channels = new();
        public override async Task OnConnectedAsync()
        {
            try
            {
                var httpContext = Context.GetHttpContext();
                var channelName = httpContext?.Request.Query["channel"];
                await Groups.AddToGroupAsync(Context.ConnectionId, channelName.Value);
                await base.OnConnectedAsync();

                _channels.TryGetValue(channelName, out var Channel);
                var episodeJson = JsonSerializer.Serialize(Channel?.Episode);
                await Clients.Client(Context.ConnectionId).SendAsync("ReceiveEpisode", episodeJson);
            }
            catch(Exception ex)
            {
                Log.Error(ex, "Error in OnConnectedAsync");
            }
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            try
            {
                foreach (var channel in _channels)
                    await Groups.RemoveFromGroupAsync(Context.ConnectionId, channel.Value.ChannelName); //Remove all groups

                await base.OnDisconnectedAsync(exception);
            }
            catch(Exception ex)
            {
                Log.Error(ex, "Error in OnDisconnectedAsync");
            }
        }

        #region Channel Methods
        public static void AddOrUpdateChannel(ChannelHub channel)
        {
            _channels.AddOrUpdate(channel.ChannelName, channel, (_, exist) => channel with
            {
                Episode = channel.Episode,
                ElapsedTime = channel.ElapsedTime
            });
        }

        public static ChannelHub? GetChannel(string ChannelName)
        {
            _channels.TryGetValue(ChannelName, out var Channel);
            return Channel;
        }

        public static IEnumerable<KeyValuePair<string, ChannelHub>> GetAllChannels()
        {
            return _channels.ToArray();
        }

        #endregion

        #region Hub Methods

        public async Task GetRandomEpisode(string channelName)
        {
            try
            {
                if(_channels.TryGetValue(channelName, out var Channel))
                {
                    var episodeJson = JsonSerializer.Serialize(Channel?.Episode);
                    await Clients.Client(Context.ConnectionId).SendAsync("ReceiveEpisode", episodeJson);
                    Log.Information($"Sending Episode for channel {channelName}. Episode Data: {episodeJson}");
                }
                else
                {
                    await Clients.Caller.SendAsync("Error", "Channel not found");
                    Log.Warning($"Channel {channelName} not found", channelName);
                }
            }catch(Exception ex)
            {
                await Clients.Caller.SendAsync("Error", "Error getting episode");
                Log.Error(ex, $"Error in GetRandomEpisode for channel {channelName}");
            }
        }

        public async Task GetElapsedTime(string channelName)
        {
            try
            {
                if (_channels.TryGetValue(channelName, out var Channel))
                {
                    await Clients.Client(Context.ConnectionId).SendAsync("ElapsedTime", Channel?.ElapsedTime);
                }
                else
                {
                    await Clients.Caller.SendAsync("Error", "Channel not found");
                    Log.Warning($"error in GetElapsedTime for channel {channelName}");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error in GetElapsedTime for channel {channelName}");
            }
        }
        #endregion
    }
}
