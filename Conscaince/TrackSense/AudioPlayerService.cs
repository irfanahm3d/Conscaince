using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Media.Playback;

namespace Conscaince.TrackSense
{
    class AudioPlayerService
    {
        static AudioPlayerService audioPlayerInstance;

        public static AudioPlayerService AudioPlayerInstance
        {
            get
            {
                if (audioPlayerInstance == null)
                    audioPlayerInstance = new AudioPlayerService();

                return audioPlayerInstance;
            }
        }

        public IDictionary<string, MediaPlayer> SoundTracks { get; private set; }

        /// <summary>
        /// A list of items to be played for the base audio track
        /// and the interrupt track
        /// </summary>
        TrackList SoundsList { get; set; }

        public AudioPlayerService()
        {
            this.SoundsList = new TrackList();
            this.SoundTracks = new Dictionary<string, MediaPlayer>();
        }

        public async Task InitializeSoundTracks()
        {
            await this.SoundsList.CreateTrackPlaybackList();

            foreach (var track in this.SoundsList.Tracks)
            {
                MediaPlayer soundEffectTrack = new MediaPlayer
                {
                    AutoPlay = track.AutoPlay,
                    IsLoopingEnabled = track.Loop,
                    Source = track.ToPlaybackItem(),
                    Volume = track.Volume             
                };

                soundEffectTrack.CurrentStateChanged += AudioTrackPlayStateChange;
                this.SoundTracks.Add(track.Title, soundEffectTrack);
            }
        }
        
        /// <summary>
        /// Plays the track based on the source title.
        /// </summary>
        /// <param name="sourceTitle">The title of the source item.</param>
        /// <returns></returns>
        public async Task<bool> Play(string sourceTitle, bool loop)
        {
            bool result = false;
            MediaPlayer soundEffectTrack;
            if (this.SoundTracks.TryGetValue(sourceTitle, out soundEffectTrack))
            {
                if (soundEffectTrack.PlaybackSession.PlaybackState != MediaPlaybackState.Playing)
                {
                    soundEffectTrack.IsLoopingEnabled = loop;
                    FadeMedia(1.0d * soundEffectTrack.Volume, soundEffectTrack);
                    soundEffectTrack.Play();
                }

                result = true;
            }

            return result;
        }

        /// <summary>
        /// Pauses the track based on the source title.
        /// </summary>
        /// <param name="sourceTitle">The title of the source item.</param>
        /// <returns></returns>
        public async Task<bool> Pause(string sourceTitle)
        {
            bool result = false;
            MediaPlayer soundEffectTrack;
            if (this.SoundTracks.TryGetValue(sourceTitle, out soundEffectTrack))
            {
                FadeMedia(-1.0d * soundEffectTrack.Volume, soundEffectTrack);
                soundEffectTrack.Pause();
                result = true;
            }

            return result;
        }

        public void Dispose()
        {
            foreach (var track in SoundTracks.Values)
            {
                track.Dispose();
            }
        }

        void AudioTrackPlayStateChange(MediaPlayer sender, object args)
        {
            // If the media item has completed and is paused then call the 
            // playstate change handler method to carry out its logic
            if (sender.PlaybackSession.PlaybackState == MediaPlaybackState.Paused &&
                sender.PlaybackSession.Position == sender.PlaybackSession.NaturalDuration)
            {
                HandleInterruptAudioPlayStateChangeEvent();
            }
        }

        async void HandleInterruptAudioPlayStateChangeEvent()
        {
        }

        async Task FadeMedia(double fadeType, MediaPlayer player)
        {
            double fadeOutSeconds = 3.0d;
            Stopwatch watch = new Stopwatch();
            watch.Start();
            while (watch.Elapsed < TimeSpan.FromSeconds(fadeOutSeconds))
            {
                if (watch.ElapsedMilliseconds % 500 == 0)
                {
                    double fraction = 1.0d / (fadeOutSeconds * 2) * fadeType;
                    double volumeLevel = player.Volume + fraction;
                    player.Volume = volumeLevel;
                }
            }
            watch.Stop();
        }
    }
}
