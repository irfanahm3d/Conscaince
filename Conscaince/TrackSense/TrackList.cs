using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Media.Core;
using Windows.Media.Playback;

namespace Conscaince.TrackSense
{
    class TrackList
    {
        JsonReader jsonReader = JsonReader.JsonReaderInstance;
        
        public IList<AudioTrack> Tracks { get; private set; }

        public TrackList()
        {
            this.Tracks = new List<AudioTrack>();
        }

        public async Task CreateTrackPlaybackList()
        {
            this.Tracks = await LoadAudioTracksFromJson();
        }

        async Task<IList<AudioTrack>> LoadAudioTracksFromJson()
        {
            IList<AudioTrack> audioList = new List<AudioTrack>();
            foreach (var jsonItem in jsonReader.BaseTrackArray)
            {
                audioList.Add(await LoadAudioTrack(jsonItem.GetObject()));
            }

            return audioList;
        }
        
        async Task<AudioTrack> LoadAudioTrack(JsonObject json)
        {
            return new AudioTrack(json);
        }
    }
}
