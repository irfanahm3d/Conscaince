using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Media.SpeechSynthesis;

namespace Conscaince.TrackSense
{
    class Synthesizer
    {
        JsonReader jsonReader = JsonReader.JsonReaderInstance;

        public IList<SynthTrack> Speeches { get; private set; }

        public Synthesizer()
        {
            this.Speeches = new List<SynthTrack>();
        }

        public async Task CreateSynthesizedVoicePlaybackList()
        {
            this.Speeches = await CreateSynthTracksFromJson();
        }

        async Task<IList<SynthTrack>> CreateSynthTracksFromJson()
        {
            IList<SynthTrack> synthList = new List<SynthTrack>();
            foreach (var jsonItem in jsonReader.SpeechArray)
            {
                synthList.Add(await LoadSynthTrack(jsonItem.GetObject()));
            }

            return synthList;
        }

        async Task<SynthTrack> LoadSynthTrack(JsonObject json)
        {
            return new SynthTrack(json);
        }
    }
}
