﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using LambdaUI.Constants;
using LambdaUI.Logging;
using LambdaUI.Models.Tempus.DetailedMapList;
using LambdaUI.Models.Tempus.Rank;
using LambdaUI.Models.Tempus.Responses;
using Newtonsoft.Json;

namespace LambdaUI.Data.Access
{
    public class TempusDataAccess
    {
        private static readonly Stopwatch Stopwatch = new Stopwatch();
        private List<DetailedMapOverviewModel> _mapList;
        private List<MapFullOverviewModel> _fullOverviewCache = new List<MapFullOverviewModel>(TempusConstants.FullMapOverviewCacheSize);

        public List<DetailedMapOverviewModel> MapList
        {
            get
            {
                if (_mapList != null) return _mapList;
                UpdateMapListAsync().GetAwaiter().GetResult();
                return MapList;
            }
            private set => _mapList = value;
        }

        private void AddMapOverviewCacheItem(MapFullOverviewModel fullOverview)
        {
            _fullOverviewCache.Insert(0, fullOverview);
            var countToRemove = _fullOverviewCache.Count - TempusConstants.FullMapOverviewCacheSize;
            if (countToRemove <= 0) return;
            _fullOverviewCache.RemoveRange(TempusConstants.FullMapOverviewCacheSize - 1, _fullOverviewCache.Count - TempusConstants.FullMapOverviewCacheSize);

        }
        public List<string> MapNameList { get; set; }

        private static HttpWebRequest CreateWebRequest(string path) => (HttpWebRequest) WebRequest.Create(
            "https://tempus.xyz/api" + path);

        private static HttpWebRequest BuildWebRequest(string relativePath)
        {
            var httpWebRequest = CreateWebRequest(relativePath);
            httpWebRequest.Method = WebRequestMethods.Http.Get;
            httpWebRequest.Accept = "application/json";
            return httpWebRequest;
        }


        private static async Task<T> GetResponseAsync<T>(string request)
        {
            Stopwatch.Restart();
            try
            {
                object stringValue;
                using (var response = (HttpWebResponse) await BuildWebRequest(request).GetResponseAsync())
                {
                    using (var stream = response.GetResponseStream())
                    {
                        stringValue = null;
                        if (stream != null)
                        {
                            using (var sr = new StreamReader(stream, Encoding.UTF8))
                            {
                                stringValue = sr.ReadToEnd();
                                sr.Close();
                            }
                            stream.Close();
                        }
                    }
                    response.Close();
                }
                Stopwatch.Stop();
                Logger.LogInfo("Tempus", "/api" + request + " " + Stopwatch.ElapsedMilliseconds + "ms");
                // If T is a string, don't deserialise
                return typeof(T) == typeof(string)
                    ? (T) stringValue
                    : JsonConvert.DeserializeObject<T>((string) stringValue);
            }
            catch (Exception e)
            {
                Logger.LogException(e);
                return default(T);
            }
        }

        public async Task<MapFullOverviewModel> GetFullMapOverViewAsync(string map)
        {
            try
            {
                return _fullOverviewCache.First(x => x.MapInfo.Name == ParseMapName(map));
            }
            catch 
            {
                // The map isn't in the cache
                var overview = await
                    GetResponseAsync<MapFullOverviewModel>($"/maps/name/{ParseMapName(map)}/fullOverview");
                AddMapOverviewCacheItem(overview);
                return overview;

            }

        }

        public async Task<RecentActivityModel> GetRecentActivityAsync() =>
            await GetResponseAsync<RecentActivityModel>("/activity");

        public async Task<List<ServerStatusModel>> GetServerStatusAsync() =>
            await GetResponseAsync<List<ServerStatusModel>>("/servers/statusList");

        public async Task<List<ShortMapInfoModel>> GetMapListAsync() =>
            await GetResponseAsync<List<ShortMapInfoModel>>("/maps/list");

        public async Task<List<DetailedMapOverviewModel>> GetDetailedMapListAsync() =>
            await GetResponseAsync<List<DetailedMapOverviewModel>>("/maps/detailedList");

        public async Task<Rank> GetUserRankAsync(string id) => await GetResponseAsync<Rank>($"/players/id/{id}/rank");


        private string ParseMapName(string map)
        {
            map = map.ToLower();
            if (MapNameList.Contains(map)) return map;

            foreach (var mapName in MapNameList)
            {
                var mapParts = mapName.Split('_');
                if (mapParts.Any(mapPart => map == mapPart)) return mapName;
            }

            throw new Exception("Map not found");
        }

        public async Task UpdateMapListAsync()
        {
            var maps = await GetDetailedMapListAsync();
            MapList = maps;
            MapNameList = maps.ConvertAll(x => x.Name).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
        }
    }
}