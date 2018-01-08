using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Media.Playback;

namespace Conscaince.TrackSense
{
    class AudioPlayerService
    {
        const double maxSound = 0.77d;
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

        public List<MediaPlayer> SoundTracks { get; private set; }

        /// <summary>
        /// A list of items to be played for the base audio track
        /// and the interrupt track
        /// </summary>
        TrackList SoundsList { get; set; }

        public AudioPlayerService()
        {
            this.SoundsList = new TrackList();
            this.SoundTracks = new List<MediaPlayer>();
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
                
                this.SoundTracks.Add(soundEffectTrack);
            }
        }
        
        public async Task Play(MediaPlayer player)
        {
            // if the BasePlaybackList set to the Source of player is not set
            // the play will not work. 
            // TODO: need to create a check for this.
            player.Play();
        }
        
        public async Task Pause(MediaPlayer player)
        {
            await FadeMedia(-1.0d * maxSound, player);
            player.Pause();
        }

        //public async Task<bool> ToggleMute()
        //{
        //    bool isMuted;
        //    if (this.Player.IsMuted)
        //    {
        //        isMuted = false;
        //        this.Player.IsMuted = isMuted;
        //        await FadeMedia(1.0d * maxSound, this.Player);
        //        await FadeMedia(1.0d * maxSound, this.Interrupter);
        //    }
        //    else
        //    {
        //        isMuted = true;
        //        await FadeMedia(-1.0d * maxSound, this.Player);
        //        this.Player.IsMuted = isMuted;
        //        this.Interrupter.Volume = 0.33;
        //    }

        //    return isMuted;
        //}

        public void Dispose()
        {
            foreach (var player in SoundTracks)
            {
                player.Dispose();
            }
        }

        //void InterruptAudioPlayStateChange(MediaPlayer sender, object args)
        //{
        //    // If the media item has completed and is paused then call the 
        //    // playstate change handler method to carry out its logic
        //    if (sender.PlaybackSession.PlaybackState == MediaPlaybackState.Paused &&
        //        sender.PlaybackSession.Position == sender.PlaybackSession.NaturalDuration)
        //    {
        //        HandleInterruptAudioPlayStateChangeEvent();
        //    }
        //}

        //async void HandleInterruptAudioPlayStateChangeEvent()
        //{
        //    // On completion of the interrupt audio the audio player should resume
        //    // playing
        //    this.Interrupter.IsMuted = true;
        //    this.Player.Play();
        //    await FadeMedia(1.0d * maxSound, this.Player);
        //}

        Task FadeMedia(double fadeType, MediaPlayer player)
        {
            double fadeOutSeconds = 5.0d;
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

            return Task.CompletedTask;
        }
    }
}
