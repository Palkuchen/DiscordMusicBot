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
            var userVC = ctx.Member.VoiceState.Channel;
            playFirstTrack(query, userVC, ctx);
        }

        [Command("pause")]
        public async Task PauseMusic(CommandContext ctx)
        {
            //PRE-EXECUTION CHECKS
            await checkPreExecution(ctx);

            var userVC = ctx.Member.VoiceState.Channel;
            var lavalinkInstance = ctx.Client.GetLavalink();

            var node = lavalinkInstance.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn == null)
            {
                await ctx.Channel.SendMessageAsync("Lavalink Failed to connect!!!");
                return;
            }

            if (conn.CurrentState.CurrentTrack == null)
            {
                await ctx.Channel.SendMessageAsync("No tracks are playing!!!");
                return;
            }

            await conn.PauseAsync();

            var pausedEmbed = new DiscordEmbedBuilder()
            {
                Color = DiscordColor.Yellow,
                Title = "Track Paused!!"
            };

            await ctx.Channel.SendMessageAsync(embed: pausedEmbed);
        }

        [Command("resume")]
        public async Task ResumeMusic(CommandContext ctx)
        {
            //PRE-EXECUTION CHECKS
            await checkPreExecution(ctx);

            var userVC = ctx.Member.VoiceState.Channel;
            var lavalinkInstance = ctx.Client.GetLavalink();

            var node = lavalinkInstance.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn == null)
            {
                await ctx.Channel.SendMessageAsync("Lavalink Failed to connect!!!");
                return;
            }

            if (conn.CurrentState.CurrentTrack == null)
            {
                await ctx.Channel.SendMessageAsync("No tracks are playing!!!");
                return;
            }

            await conn.ResumeAsync();

            var resumedEmbed = new DiscordEmbedBuilder()
            {
                Color = DiscordColor.Green,
                Title = "Resumed"
            };

            await ctx.Channel.SendMessageAsync(embed: resumedEmbed);
        }

        [Command("stop")]
        public async Task StopMusic(CommandContext ctx)
        {

            //PRE-EXECUTION CHECKS
            await checkPreExecution(ctx);

            var userVC = ctx.Member.VoiceState.Channel;
            var lavalinkInstance = ctx.Client.GetLavalink();

            var node = lavalinkInstance.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn == null)
            {
                await ctx.Channel.SendMessageAsync("Lavalink Failed to connect!!!");
                return;
            }

            if (conn.CurrentState.CurrentTrack == null)
            {
                await ctx.Channel.SendMessageAsync("No tracks are playing!!!");
                return;
            }

            await conn.StopAsync();
            await conn.DisconnectAsync();

            var stopEmbed = new DiscordEmbedBuilder()
            {
                Color = DiscordColor.Red,
                Title = "Stopped the Track",
                Description = "Successfully disconnected from the VC"
            };

            await ctx.Channel.SendMessageAsync(embed: stopEmbed);
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

        public DiscordEmbedBuilder getMusicEmblem(LavalinkTrack musicTrack, DiscordChannel userVC)
        {
            string musicDescription = $"Now Playing: {musicTrack.Title} \n" +
                                      $"Author: {musicTrack.Author} \n" +
                                      $"length: {musicTrack.Length} \n" +
                                      $"URL: {musicTrack.Uri}";

            var nowPlayingEmbed = new DiscordEmbedBuilder()
            {
                Color = DiscordColor.Purple,
                Title = $"Successfully joined channel {userVC.Name} and playing music",
                Description = musicDescription
            };

            return nowPlayingEmbed;
        }
        public DiscordEmbedBuilder getQueueEmblem(LavalinkTrack musicTrack, DiscordChannel userVC)
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

        // Plays first track found in Grid
        public async void playFirstTrack(String query, DiscordChannel userVC, CommandContext ctx)
        {
            var lavalinkInstance = ctx.Client.GetLavalink();
            var node = lavalinkInstance.ConnectedNodes.Values.First();
            await node.ConnectAsync(userVC);

            textChannel = ctx.Channel;

            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);
            if (conn == null)
            {
                await textChannel.SendMessageAsync("Lavalink Failed to connect!!!");
                return;
            }

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
                await ctx.Channel.SendMessageAsync(embed: getQueueEmblem(musicTrack, userVC));
                return;
            }

            await conn.PlayAsync(musicTrack);
            conn.PlaybackFinished += Conn_PlaybackFinished;

            await ctx.Channel.SendMessageAsync(embed: getMusicEmblem(musicTrack, userVC));
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

            await textChannel.SendMessageAsync(embed: getMusicEmblem(musicTrack, textChannel));
        }
    }
}