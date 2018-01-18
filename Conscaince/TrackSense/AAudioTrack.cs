using System;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Media.Playback;

namespace Conscaince.TrackSense
{
    abstract class AAudioTrack
    {
        public string Title { get; set; }

        public AAudioTrack()
        {
        }

        public AAudioTrack(JsonObject json)
        {
            Title = json.GetNamedString("title", string.Empty);
        }

        public async Task<MediaPlaybackItem> ToPlaybackItem() => throw new NotImplementedException();
    }
}
