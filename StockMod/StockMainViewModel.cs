using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ExtenderApp.Abstract;
using ExtenderApp.Common;
using ExtenderApp.ViewModels;
using StockMod.Data;

namespace StockMod
{
    public class StockMainViewModel : ExtenderAppViewModel
    {
        private string myToken = "bdf712b758795a1a76758186891dbb13-c-app";
        string testUrl = "https://quote.tradeswitcher.com/quote-stock-b-api/kline?token=testtoken&query=Query";
        private readonly INetworkClient _networkClient;

        public StockMainViewModel(IJsonPareserProvider provider, INetworkClient client, IServiceStore service) : base(service)
        {
            string requestUrl = testUrl.Replace("testtoken", myToken); // 替换令牌
            _networkClient = client;
            //StockRequestMessage message = new StockRequestMessage("Test_1", new()
            //{
            //    Code = "700.HK",
            //    KlineType = 1,
            //    KlineTimestampEnd = 0,
            //    QueryKlineNum = 2,
            //    AdjustType = 0
            //});
            //var mes = provider.Serialize(message.ToJson());

            //var query = WebUtility.UrlEncode(mes);
            //requestUrl = requestUrl.Replace("Query", query);
            //Get(requestUrl);
        }

        private async void Get(string url)
        {
            var reslut = await _networkClient.GetAsync(url);
            Debug.Print(reslut.Content.ReadAsStringAsync().Result);
        }
    }
}
