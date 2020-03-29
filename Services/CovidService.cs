using Covid19.Models;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Covid19.Services
{

    public interface ICovidService
    {
        Task<Dictionary<string, EventModel>> GetCovidAsync();
        Task<object> GetDayWiseCountryDetailsAsync(string country);
    }

    public sealed class CovidService : ICovidService
    {

        private readonly IMemoryCache _memoryCache;
        private readonly IHttpClientFactory _httpClientFactory;
        private static Uri BaseUrl = new Uri("https://pomber.github.io/covid19/timeseries.json");

        public CovidService(IHttpClientFactory httpClientFactory, IMemoryCache memoryCache )
        {
            _httpClientFactory = httpClientFactory;
            _memoryCache = memoryCache;
        }

        private async Task<IDictionary<string, EventModel[]>> GetCovidResourceAsync()
        {
            var cacheData = CheckIfCacheDataExists("Covid19Data");

            if(cacheData != null)
            {
                return cacheData;
            }

            var httpClient = _httpClientFactory.CreateClient();

            using (var req = new HttpRequestMessage(HttpMethod.Get, BaseUrl))
            {
                req.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                using (var reqRes = await httpClient.SendAsync(req).ConfigureAwait(false))
                {
                    reqRes.EnsureSuccessStatusCode();
                    var retData =  await reqRes.Content.ReadAsAsync<ImmutableDictionary<string, EventModel[]>>();

                    _memoryCache.Set("Covid19Data", retData, TimeSpan.FromHours(3));

                    return retData;
                }
            }
        }

        private IDictionary<string, EventModel[]> CheckIfCacheDataExists(string cacheKey)
        {
            if (_memoryCache.TryGetValue(cacheKey, out IDictionary<string, EventModel[]> cacheEntry))
            {
                return cacheEntry;
            }

            return null;
        }

        public async Task<Dictionary<string, EventModel>>  GetCovidAsync()
        {
            var ret = await GetCovidResourceAsync();
            var computeData = new Dictionary<string, EventModel>();

            var retData = ret.OrderBy(x => x.Key);

            foreach (KeyValuePair<string, EventModel[]> entry in retData)
            {
                var eventModelData = entry.Value[entry.Value.Length - 1];
                computeData.Add(entry.Key, eventModelData);
            }

            return computeData;
        }

        public async Task<object> GetDayWiseCountryDetailsAsync(string country)
        {
            var retData = await GetCovidResourceAsync();

            var computeData = retData
                                .Where(x => country.Contains(x.Key))
                                .Select(x => x.Value);
            computeData = computeData.ToList();
            return computeData;
        }
    }
}
