using ExtenderApp.Common.Caches;

namespace ExtenderApp.Torrent.Models.Trackers
{
    public class TrackerResponseCache : EvictionCache<InfoHash, TrackerResponse>
    {
        protected override bool ShouldEvict(TrackerResponse value, DateTime now)
        {
            return value.LastAnnounceTime - now > TimeSpan.FromSeconds(value.Interval);
        }

        protected override void Evict(InfoHash key, TrackerResponse value)
        {
            value.Release();
        }
    }
}
