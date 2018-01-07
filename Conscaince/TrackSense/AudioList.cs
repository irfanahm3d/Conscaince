using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Media.Playback;

namespace Conscaince.TrackSense
{
    class AudioList
    {
        JsonReader jsonReader = JsonReader.JsonReaderInstance;

        public MediaPlaybackList BasePlaybackList { get; private set; }
        public MediaPlaybackList InterruptPlaybackList { get; private set; }

        //public IList<InterruptTrack> InterruptTimings { get; private set; }

        public AudioList()
        {
            //this.InterruptTimings = new List<InterruptTrack>();
        }

        public async Task CreateAudioPlaybackLists()
        {
            IList<AudioTrack> tempAudioList = LoadAudioTracks();
            //IList<InterruptTrack> tempInterruptList = LoadInterruptTracks();
            
            this.BasePlaybackList = await ConvertToMediaPlaybackList(tempAudioList);
            //this.InterruptPlaybackList = await ConvertToMediaPlaybackList(tempInterruptList, false, false);
        }

        async Task<MediaPlaybackList> ConvertToMediaPlaybackList<T>(
            IList<T> audioList,
            bool autoRepeatEnabled = true,
            bool shuffleEnabled = true)
            where T : AAudioTrack
        {
            var playbackList = new MediaPlaybackList();

            // Make a new list and enable looping and shuffling
            playbackList.AutoRepeatEnabled = autoRepeatEnabled;
            playbackList.ShuffleEnabled = shuffleEnabled;

            // Add playback items to the list
            foreach (var audioTrack in audioList)
            {
                playbackList.Items.Add(audioTrack.ToPlaybackItem());
            }

            return playbackList;
        }

        IList<AudioTrack> LoadAudioTracks()
        {
            IList<AudioTrack> audioList = new List<AudioTrack>();
            foreach (var jsonItem in jsonReader.BaseTrackArray)
            {
                audioList.Add(LoadAudioTrack(jsonItem.GetObject()));
            }

            return audioList;
        }

        //IList<InterruptTrack> LoadInterruptTracks()
        //{
        //    IList<InterruptTrack> interruptList = new List<InterruptTrack>();
        //    foreach (var jsonItem in jsonReader.InterruptTrackArray)
        //    {
        //        interruptList.Add(LoadInterruptTrack(jsonItem.GetObject()));
        //    }

        //    return interruptList;
        //}

        AudioTrack LoadAudioTrack(JsonObject json)
        {
            return new AudioTrack(json);
        }

        //InterruptTrack LoadInterruptTrack(JsonObject json)
        //{
        //    var interruptTrack = new InterruptTrack(json);
        //    this.InterruptTimings.Add(interruptTrack);

        //    return interruptTrack;
        //}
    }
}
