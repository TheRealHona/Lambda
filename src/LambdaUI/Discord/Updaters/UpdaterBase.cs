﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using LambdaUI.Logging;

namespace LambdaUI.Discord.Updaters
{
    public class UpdaterBase
    {
        protected static async Task DeleteAllMessagesAsync(ITextChannel channel)
        {
            try
            {
                var messages = await channel.GetMessagesAsync().FlattenAsync();
                await channel.DeleteMessagesAsync(messages);
            }
            catch (Exception e)
            {
                await channel.SendMessageAsync(embed: Logger.LogException(e));
            }
        }

    }
}