


using ExtenderApp.Data;

namespace ExtenderApp.Torrent
{
    public class Tracker : ITracker
    {
        public bool CanScrape => throw new NotImplementedException();

        public Uri Uri => throw new NotImplementedException();

        public LinkState Status => throw new NotImplementedException();
    }
}
