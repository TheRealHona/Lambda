﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using LambdaUI.Data.Access;
using LambdaUI.Data.Access.Bot;
using LambdaUI.Logging;
using LambdaUI.Services;

namespace LambdaUI.Discord.Updaters
{
    public class TempusServerUpdater : UpdaterBase
    {
        private readonly DiscordSocketClient _client;
        private readonly ConfigDataAccess _configDataAccess;
        private readonly TempusDataAccess _tempusDataAccess;

        public TempusServerUpdater(DiscordSocketClient client, ConfigDataAccess configDataAccess,
            TempusDataAccess tempusDataAccess)
        {
            _client = client;
            _configDataAccess = configDataAccess;
            _tempusDataAccess = tempusDataAccess;
        }

        public async Task UpdateOverviewsAsync()
        {
            var updateChannels = await _configDataAccess.GetConfigAsync("tempusOverviewChannel");
            if (updateChannels == null || updateChannels.Count == 0) return;
            foreach (var updateChannel in updateChannels)
                await UpdateChannelOverviewAsync(updateChannel.Value);
        }

        private async Task UpdateChannelOverviewAsync(string updateChannel)
        {
            if (!(_client.GetChannel(Convert.ToUInt64(updateChannel)) is ITextChannel channel)) return;
            try
            {

                var embeds = new List<Embed>
                {
                    TempusServerStatusService.GetServerStatusOverviewEmbed(
                        await _tempusDataAccess.GetServerStatusAsync()),
                    await TempusApiService.UpdateStalkTopEmbedAsync(_tempusDataAccess)
                };

                var existingMessages = (await channel.GetMessagesAsync().FlattenAsync()).ToList();

                foreach (var embed in embeds)
                {
                    var existingMessage = existingMessages.FirstOrDefault(x => x.Embeds.Count == 1 && x.Embeds.First().Title == embed.Title);

                    if (existingMessage != null && existingMessage is IUserMessage userMessage)
                    {
                        await userMessage.ModifyAsync(message => message.Embed = embed);
                        existingMessages.Remove(existingMessage);
                    }
                    else
                    {
                        await channel.SendMessageAsync(embed: embed);
                    }
                }

                if (existingMessages.Count > 0)
                {
                    foreach (var oldMessage in existingMessages)
                    {
                        await oldMessage.DeleteAsync();
                    }
                }
            }
            catch (Exception e)
            {
                await channel.SendMessageAsync(embed: Logger.LogException(e));
            }
        }

        public async Task UpdateServersAsync()
        {
            var updateChannels = await _configDataAccess.GetConfigAsync("tempusUpdateChannel");
            if (updateChannels == null || updateChannels.Count == 0) return;
            foreach (var updateChannel in updateChannels)
                await UpdateChannelAsync(updateChannel.Value);
        }

        private async Task UpdateChannelAsync(string updateChannel)
        {
            if (!(_client.GetChannel(Convert.ToUInt64(updateChannel)) is ITextChannel channel)) return;
            try
            {
                var serverInfo = await _tempusDataAccess.GetServerStatusAsync();
                var embeds = serverInfo.Select(TempusServerStatusService.GetServerStatusAsync).Where(x=>x != null).ToList();

                var existingMessages = (await channel.GetMessagesAsync().FlattenAsync()).ToList();

                var count = 0;
                foreach (var embed in embeds)
                {
                    if (count % 5 == 0)
                    {
                        await Task.Delay(4200);
                    }
                    var existingMessage = existingMessages.FirstOrDefault(x =>
                        x.Embeds.Count == 1 && x.Embeds.First().Title == embed.Title);

                    if (existingMessage != null && existingMessage is IUserMessage userMessage)
                    {
                        await userMessage.ModifyAsync(message => message.Embed = embed);
                        existingMessages.Remove(existingMessage);
                    }
                    else
                    {
                        await channel.SendMessageAsync(embed: embed);
                    }

                    count++;
                }

                if (existingMessages.Count > 0)
                {
                    foreach (var oldMessage in existingMessages)
                    {
                        await oldMessage.DeleteAsync();
                    }
                }

                
            }
            catch (Exception e)
            {
                await channel.SendMessageAsync(embed: Logger.LogException(e));
            }
        }
    }
}