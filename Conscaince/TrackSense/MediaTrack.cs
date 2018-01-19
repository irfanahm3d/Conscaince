using Windows.Media.Playback;

namespace Conscaince.TrackSense
{
    class MediaTrack
    {
        public MediaPlayer Player { get; private set; }

        public bool IsLooping { get; private set; }

        public double BaseVolume { get; private set; }

        public MediaTrack()
        {
        }

        public MediaTrack(MediaPlayer player, bool loop, double volume)
        {
            this.Player = player;
            this.IsLooping = loop;
            this.BaseVolume = volume;
        }
    }
}
