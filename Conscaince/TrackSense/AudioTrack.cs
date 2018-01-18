using System;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Media;
using Windows.Media.Core;
using Windows.Media.Playback;

namespace Conscaince.TrackSense
{
    class AudioTrack : AAudioTrack
    {
        public bool AutoPlay { get; set; }

        public bool Loop { get; set; }

        public string ItemId { get; set; }

        public Uri MediaUri { get; set; }

        public double Volume { get; set; }

        public AudioTrack(JsonObject json) : base(json)
        {
            Volume = json.GetNamedNumber("volume", 0.5d);
            AutoPlay = json.GetNamedBoolean("autoplay", false);
            Loop = json.GetNamedBoolean("loop", false);

            if (json.Keys.Contains("mediaUri"))
                MediaUri = new Uri(json.GetNamedString("mediaUri"));
        }

        public async Task<MediaPlaybackItem> ToPlaybackItem()
        {
            // Create the media source from the Uri
            var source = MediaSource.CreateFromUri(MediaUri);

            // Create a configurable playback item backed by the media source
            var playbackItem = new MediaPlaybackItem(source);

            MediaItemDisplayProperties displayProperties = playbackItem.GetDisplayProperties();
            displayProperties.Type = MediaPlaybackType.Music;
            displayProperties.MusicProperties.Title = this.Title;
            playbackItem.ApplyDisplayProperties(displayProperties);

            return playbackItem;
        }
    }
}
