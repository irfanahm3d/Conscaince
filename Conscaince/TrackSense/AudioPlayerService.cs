using System;
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

        /// <summary>
        /// The base audio track media player
        /// </summary>
        public MediaPlayer Player { get; private set; }

        /// <summary>
        /// Plays media that will interrupt the base audio track
        /// </summary>
        public MediaPlayer Interrupter { get; private set; }
        
        //private InterrupterService interrupt;

        public AudioPlayerService()
        {
        }

        public async Task InitializeAudioPlayers(AudioList audioList)
        {
            this.Player = new MediaPlayer();
            this.Player.Volume = maxSound;
            this.Player.Source = audioList.BasePlaybackList;

            this.Interrupter = new MediaPlayer();
            this.Interrupter.AutoPlay = false;
            this.Interrupter.IsLoopingEnabled = false;
            this.Interrupter.Source = audioList.InterruptPlaybackList;
            this.Interrupter.CurrentStateChanged += InterruptAudioPlayStateChange;

            //this.interrupt = new InterrupterService(audioList.InterruptTimings);
            //this.interrupt.OnTimeTriggered += InterruptAudio;
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

        public async Task<bool> ToggleMute()
        {
            bool isMuted;
            if (this.Player.IsMuted)
            {
                isMuted = false;
                this.Player.IsMuted = isMuted;
                await FadeMedia(1.0d * maxSound, this.Player);
                await FadeMedia(1.0d * maxSound, this.Interrupter);
            }
            else
            {
                isMuted = true;
                await FadeMedia(-1.0d * maxSound, this.Player);
                this.Player.IsMuted = isMuted;
                this.Interrupter.Volume = 0.33;
            }

            return isMuted;
        }

        public void Dispose()
        {
            this.Player.Dispose();
        }

        async void InterruptAudio()
        {
            await FadeMedia(-1.0d * maxSound, this.Player);
            this.Player.Pause();
            this.Interrupter.IsMuted = false;
            this.Interrupter.Play();
        }

        void InterruptAudioPlayStateChange(MediaPlayer sender, object args)
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
            // On completion of the interrupt audio the audio player should resume
            // playing
            this.Interrupter.IsMuted = true;
            this.Player.Play();
            await FadeMedia(1.0d * maxSound, this.Player);
        }

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
