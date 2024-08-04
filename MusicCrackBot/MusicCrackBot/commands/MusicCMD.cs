using Discord;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.Entities;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;

namespace MusicCrackBot.commands
{
    internal class MusicCMD : BaseCommandModule
    {

        public List<LavalinkTrack> songs = new List<LavalinkTrack>();
        public DiscordChannel textChannel;

        [Command("play")]
        public async Task PlayMusic(CommandContext ctx, [RemainingText] string query)
        {
            await checkPreExecution(ctx);
            playFirstTrack(query, ctx);
        }

        [Command("pause")]
        public async Task PauseMusic(CommandContext ctx)
        {
            //PRE-EXECUTION CHECKS
            await checkPreExecution(ctx);
            var conn = await connect(ctx);
            var musicTrack = conn.CurrentState.CurrentTrack;

            if (musicTrack == null)
            {
                await ctx.Channel.SendMessageAsync("No tracks are playing!!!");
                return;
            }

            await conn.PauseAsync();

            await ctx.Channel.SendMessageAsync(embed: getPauseEmblem(musicTrack));
        }

        [Command("resume")]
        public async Task ResumeMusic(CommandContext ctx)
        {
            //PRE-EXECUTION CHECKS
            await checkPreExecution(ctx);
            var conn = await connect(ctx);
            var musicTrack = conn.CurrentState.CurrentTrack;

            if (musicTrack == null)
            {
                await ctx.Channel.SendMessageAsync("No tracks are playing!!!");
                return;
            }

            await conn.ResumeAsync();
            await ctx.Channel.SendMessageAsync(embed: getResumeEmblem(musicTrack));
        }

        [Command("skip")]
        public async Task SkipMusic(CommandContext ctx)
        {

            //PRE-EXECUTION CHECKS
            await checkPreExecution(ctx);
            var conn = await connect(ctx);
            var musicTrack = conn.CurrentState.CurrentTrack;

            if (musicTrack == null)
            {
                await ctx.Channel.SendMessageAsync("No tracks are playing!!!");
                return;
            }

            await conn.StopAsync();      

            await ctx.Channel.SendMessageAsync(embed: getSkipEmblem(musicTrack));
        }

        [Command("stop")]
        public async Task StopMusic(CommandContext ctx)
        {

            //PRE-EXECUTION CHECKS
            await checkPreExecution(ctx);
            var conn = await connect(ctx);

            if (conn.CurrentState.CurrentTrack == null)
            {
                await ctx.Channel.SendMessageAsync("No tracks are playing!!!");
                return;
            }

            songs.Clear();
            await conn.StopAsync();

            await ctx.Channel.SendMessageAsync(embed: getStopEmblem());
        }

        [Command("leave")]
        public async Task leaveChannel(CommandContext ctx)
        {
            //PRE-EXECUTION CHECKS
            await checkPreExecution(ctx);
            var conn = await connect(ctx);

            await conn.StopAsync();
            await conn.DisconnectAsync();

            await ctx.Channel.SendMessageAsync(embed: getLeaveEmblem());
        }

        [Command("help")]
        public async Task help(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync(embed: getHelpEmblem());
        }

        [Command("volume")]
        public async Task setVolume(CommandContext ctx, [RemainingText] int volume)
        {
            var conn = await connect(ctx);
            await conn.SetVolumeAsync(volume);
            await ctx.Channel.SendMessageAsync(embed: getVolumeEmblem(volume));
        }
        public DiscordEmbedBuilder getMusicEmblem(LavalinkTrack musicTrack)
        {
            string musicDescription = $"Now Playing: {musicTrack.Title} \n" +
                                      $"Author: {musicTrack.Author} \n" +
                                      $"length: {musicTrack.Length} \n" +
                                      $"URL: {musicTrack.Uri}";

            var nowPlayingEmbed = new DiscordEmbedBuilder()
            {
                Color = DiscordColor.Purple,
                Title = $"Successfully playing music",
                Description = musicDescription
            };

            return nowPlayingEmbed;
        }
        public DiscordEmbedBuilder getQueueEmblem(LavalinkTrack musicTrack)
        {
            string musicDescription = $"Added: {musicTrack.Title} to queue \n" +
                                      $"Author: {musicTrack.Author} \n" +
                                      $"length: {musicTrack.Length} \n" +
                                      $"URL: {musicTrack.Uri}";

            var nowPlayingEmbed = new DiscordEmbedBuilder()
            {
                Color = DiscordColor.Blue,
                Title = $"Successfully added Song {musicTrack.Title} to queue",
                Description = musicDescription
            };

            return nowPlayingEmbed;
        }
        public DiscordEmbedBuilder getPauseEmblem(LavalinkTrack musicTrack)
        {
            string musicDescription = $"Track: {musicTrack.Title}\n" +
                                      $"length to go: {musicTrack.Length - musicTrack.Position}";

            var pausedEmbed = new DiscordEmbedBuilder()
            {
                Color = DiscordColor.Yellow,
                Title = $"Track Paused",
                Description = musicDescription
            };

            return pausedEmbed;
        }
        public DiscordEmbedBuilder getStopEmblem()
        {

            var stopEmbed = new DiscordEmbedBuilder()
            {
                Color = DiscordColor.Orange,
                Title = "Stopped playing tracks",
                Description = "Successfully stopped playing, leaving with !leave"
            };

            return stopEmbed;
        }
        public DiscordEmbedBuilder getVolumeEmblem(int volume)
        {

            var stopEmbed = new DiscordEmbedBuilder()
            {
                Color = DiscordColor.Green,
                Title = "Sucsesfully changed Volume",
                Description = $"Changed Volume to {volume}"
            };

            return stopEmbed;
        }
        public DiscordEmbedBuilder getLeaveEmblem()
        {

            var stopEmbed = new DiscordEmbedBuilder()
            {
                Color = DiscordColor.Red,
                Title = "Disconnected from VC",
                Description = "Successfully stopped the track and left the channeö"
            };

            return stopEmbed;
        }
        public DiscordEmbedBuilder getSkipEmblem(LavalinkTrack musicTrack)
        {

            var stopEmbed = new DiscordEmbedBuilder()
            {
                Color = DiscordColor.Green,
                Title = $"Skipped {musicTrack.Title}",
                Description = "Successfully skipped the track going on in Queue"
            };

            return stopEmbed;
        }
        public DiscordEmbedBuilder getResumeEmblem(LavalinkTrack musicTrack)
        {
            string musicDescription = $"Track: {musicTrack.Title}\n" +
                                      $"length to go: {musicTrack.Length - musicTrack.Position}";

            var pausedEmbed = new DiscordEmbedBuilder()
            {
                Color = DiscordColor.Green,
                Title = $"Track Paused",
                Description = musicDescription
            };

            return pausedEmbed;
        }
        public DiscordEmbedBuilder getHelpEmblem()
        {
            string musicDescription = $"!play [Track Titel]: plays a Track\n" +
                                      $"!skip: skipps the current Track\n" +
                                      $"!leave: Disconnects from Channel\n" +
                                      $"!resume: starts the Track after pausing\n" +
                                      $"!Pause: pauses the Track";

            var pausedEmbed = new DiscordEmbedBuilder()
            {
                Color = DiscordColor.HotPink,
                Title = $"These Commands are avalebale",
                Description = musicDescription
            };

            return pausedEmbed;
        }


        // Plays first track found in Grid
        public async void playFirstTrack(String query, CommandContext ctx)
        {
            var lavalinkInstance = ctx.Client.GetLavalink();
            var node = lavalinkInstance.ConnectedNodes.Values.First();
            await node.ConnectAsync(ctx.Member.VoiceState.Channel);

            textChannel = ctx.Channel;

            var conn = await connect(ctx);

            var searchQuery = await node.Rest.GetTracksAsync(query);
            if (searchQuery.LoadResultType == LavalinkLoadResultType.NoMatches || searchQuery.LoadResultType == LavalinkLoadResultType.LoadFailed)
            {
                await ctx.Channel.SendMessageAsync($"Failed to find music with query: {query}");
                return;
            }

            var musicTrack = searchQuery.Tracks.First();

            if (conn.CurrentState.CurrentTrack != null)
            {
                // Adding to queue
                songs.Add(musicTrack);
                await ctx.Channel.SendMessageAsync(embed: getQueueEmblem(musicTrack));
                return;
            }

            await conn.SetVolumeAsync(5);
            await conn.PlayAsync(musicTrack);
            conn.PlaybackFinished += Conn_PlaybackFinished;

            await ctx.Channel.SendMessageAsync(embed: getMusicEmblem(musicTrack));
        }

        // Could be changed to just playTrack
        public async void playQueuedTrack(LavalinkGuildConnection conn, LavalinkTrack musicTrack)
        {
            if (conn == null)
            {
                await textChannel.SendMessageAsync("Lavalink Failed to connect!!!");
                return;
            }

            await conn.PlayAsync(musicTrack);
            conn.PlaybackFinished += Conn_PlaybackFinished;

            await textChannel.SendMessageAsync(embed: getMusicEmblem(musicTrack));
        }

        public async Task<LavalinkGuildConnection> connect(CommandContext ctx)
        {
            // Hanels the connection to lavalink

            var lavalinkInstance = ctx.Client.GetLavalink();

            var node = lavalinkInstance.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn == null)
            {
                await ctx.Channel.SendMessageAsync("Lavalink Failed to connect!!!");
                return null;
            }

            return conn;
        }
        private Task Conn_PlaybackFinished(LavalinkGuildConnection sender, DSharpPlus.Lavalink.EventArgs.TrackFinishEventArgs args)
        {
            if (songs.Count > 0)
            {
                var inQueue = songs[0];
                songs.Remove(inQueue);
                playQueuedTrack(sender, inQueue);
                return Task.CompletedTask;
            }
            else
            {
                return sender.Channel.SendMessageAsync($"Nothing left in queue");
            }
        }
        public async Task checkPreExecution(CommandContext ctx)
        {
            if (ctx.Member == null)
            {
                await ctx.Channel.SendMessageAsync("F");
                return;
            }
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.Channel.SendMessageAsync("Please enter a VC!!!");
                return;
            }

            if (!ctx.Client.GetLavalink().ConnectedNodes.Any())
            {
                await ctx.Channel.SendMessageAsync("Connection is not Established!!!");
                return;
            }

            if (ctx.Member.VoiceState.Channel.Type != DSharpPlus.ChannelType.Voice)
            {
                await ctx.Channel.SendMessageAsync("Please enter a valid VC!!!");
                return;
            }

            return;
        }
    }
}