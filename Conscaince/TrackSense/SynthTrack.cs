using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Media.SpeechSynthesis;

namespace Conscaince.TrackSense
{
    class SynthTrack : AAudioTrack
    {
        public string Text { get; set; }
        public SynthTrack(JsonObject json) : base(json)
        {
            this.Text = json.GetNamedString("text", string.Empty);
        }

        public async Task<MediaPlaybackItem> ToPlaybackItem()
        {
            using (SpeechSynthesizer synth = new SpeechSynthesizer())
            {
                SpeechSynthesisStream synthesisStream = await synth.SynthesizeTextToStreamAsync(this.Text);
                return new MediaPlaybackItem(
                    MediaSource.CreateFromStream(synthesisStream, synthesisStream.ContentType));
            }
        }
    }
}
