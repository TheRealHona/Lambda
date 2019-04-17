﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using LambdaUI.Constants;
using LambdaUI.Data;
using LambdaUI.Models.Tempus.Activity;
using LambdaUI.Services;
using LambdaUI.Utilities;

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

        public async Task UpdateActivity()
        {
            var updateChannels = await _configDataAccess.GetConfigAsync("tempusActivityChannel");
            foreach (var updateChannel in updateChannels.Select(x => x.Value))
            {
                if (!(_client.GetChannel(Convert.ToUInt64(updateChannel)) is ITextChannel channel)) continue;
                await DeleteAllMessages(channel);
                var activity = await _tempusDataAccess.GetRecentActivityAsync();
                await TempusUpdaterService.SendMapRecords(activity.MapRecords, channel);
                await TempusUpdaterService.SendMapTopTimes(activity.MapTopTimes, channel);
            }
        }


    }
}