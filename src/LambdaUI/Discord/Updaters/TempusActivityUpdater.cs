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
    public class TempusActivityUpdater : UpdaterBase
    {
        private readonly DiscordSocketClient _client;
        private readonly ConfigDataAccess _configDataAccess;
        private readonly TempusDataAccess _tempusDataAccess;

        public TempusActivityUpdater(DiscordSocketClient client, ConfigDataAccess configDataAccess,
            TempusDataAccess tempusDataAccess)
        {
            _client = client;
            _configDataAccess = configDataAccess;
            _tempusDataAccess = tempusDataAccess;
        }

        public async Task UpdateActivityAsync()
        {
            var updateChannels = await _configDataAccess.GetConfigAsync("tempusActivityChannel");
            if (updateChannels == null || updateChannels.Count == 0) return;
            foreach (var updateChannel in updateChannels)
                await UpdateChannelAsync(updateChannel.Value);

        }

        private async Task UpdateChannelAsync(string updateChannel)
        {
            if (!(_client.GetChannel(Convert.ToUInt64(updateChannel)) is ITextChannel channel)) return;
            try
            {
                var activity = await _tempusDataAccess.GetRecentActivityAsync();
                var embeds = new List<Embed>
                {
                    TempusActivityService.GetMapRecordsEmbed(activity.MapRecords),
                    TempusActivityService.GetMapTopTimesEmbed(activity.MapTopTimes),
                    TempusActivityService.GetCourseRecordsEmbed(activity.CourseRecords),
                    TempusActivityService.GetBonusRecordsEmbed(activity.BonusRecords)
                };

                var existingMessages = (await channel.GetMessagesAsync().FlattenAsync()).ToList();

                foreach (var embed in embeds)
                {
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