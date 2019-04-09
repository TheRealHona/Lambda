﻿using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace LambdaUI.Models.Tempus.Responses
{
    public class ServerStatusModel
    {
        [JsonProperty(PropertyName = "game_info")]
        public GameInfo GameInfo { get; set; }
        [JsonProperty(PropertyName = "server_info")]
        public ServerInfo ServerInfo { get; set; }

        public override string ToString()
        {
            return ServerInfo.Name;
        }
    }
}