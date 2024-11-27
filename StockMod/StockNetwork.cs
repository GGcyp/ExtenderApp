using ExtenderApp.Abstract;
using StockMod.Data;

namespace StockMod
{
    internal class StockNetwork
    {
        private readonly string tockn = "bdf712b758795a1a76758186891dbb13-c-app";
        private readonly INetworkClient _client;
        private string trace;

        public StockNetwork(INetworkClient client)
        {
            _client = client;
            trace = "test_1";
        }

        public void Get()
        {

        }
    }
}
