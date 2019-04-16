﻿using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace LambdaUI.Models.Tempus.Rank
{
    public class PlayerInfo
    {
        [JsonProperty(PropertyName = "steamid")] public string Steamid { get; set; }
        [JsonProperty(PropertyName = "name")] public string Name { get; set; }
        [JsonProperty(PropertyName = "country")] public object Country { get; set; }
        [JsonProperty(PropertyName = "first_seen")] public double FirstSeen { get; set; }
        [JsonProperty(PropertyName = "id")] public int Id { get; set; }
        [JsonProperty(PropertyName = "last_seen")] public double? LastSeen { get; set; }
    }
}
