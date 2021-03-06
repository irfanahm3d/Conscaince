﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Media.Playback;

namespace Conscaince.TrackSense
{
    enum FadeType
    {
        FadeIn,
        FadeOut
    }

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

        public IDictionary<string, MediaTrack> SoundTracks { get; private set; }

        public event EventHandler AudioTrackCompleted;

        /// <summary>
        /// A list of items to be played for the base audio track
        /// and the interrupt track
        /// </summary>
        TrackList soundsList { get; set; }

        Synthesizer aiNarrator { get; set; }

        public AudioPlayerService()
        {
            this.aiNarrator = new Synthesizer();
            this.soundsList = new TrackList();
            this.SoundTracks = new Dictionary<string, MediaTrack>();
        }

        public async Task InitializeSoundTracks()
        {
            this.InitializeSoundEffects();
            this.InitializeSpeechSynthesis();
        }
        
        /// <summary>
        /// Plays the track based on the source title.
        /// </summary>
        /// <param name="sourceTitle">The title of the source item.</param>
        /// <returns></returns>
        public async Task<bool> Play(string sourceTitle, bool loop)
        {
            bool result = false;
            MediaTrack soundEffectTrack;
            if (this.SoundTracks.TryGetValue(sourceTitle, out soundEffectTrack))
            {
                if (soundEffectTrack.Player.PlaybackSession.PlaybackState != MediaPlaybackState.Playing)
                {
                    soundEffectTrack.Player.IsLoopingEnabled = loop;
                    if (!soundEffectTrack.Player.IsLoopingEnabled)
                    {
                        soundEffectTrack.Player.CurrentStateChanged += AudioTrackPlayStateChange;
                    }

                    soundEffectTrack.Player.Play();
                    FadeMedia(FadeType.FadeIn, soundEffectTrack.Player, soundEffectTrack.BaseVolume);
                }

                result = true;
            }
            else
            {
                throw new ArgumentException("file not found");
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
            MediaTrack soundEffectTrack;
            if (this.SoundTracks.TryGetValue(sourceTitle, out soundEffectTrack))
            {
                FadeMedia(FadeType.FadeOut, soundEffectTrack.Player, soundEffectTrack.BaseVolume);
                soundEffectTrack.Player.Pause();
                result = true;
            }

            return result;
        }

        public void Dispose()
        {
            foreach (var track in SoundTracks.Values)
            {
                track.Player.Dispose();
            }
        }

        async Task InitializeSoundEffects()
        {
            await this.soundsList.CreateTrackPlaybackList();

            foreach (var track in this.soundsList.Tracks)
            {
                MediaPlayer soundEffectTrack = new MediaPlayer
                {
                    AutoPlay = track.AutoPlay,
                    IsLoopingEnabled = track.Loop,
                    Source = await track.ToPlaybackItem(),
                    Volume = 0.0d
                };

                soundEffectTrack.CurrentStateChanged += AudioTrackPlayStateChange;
                this.SoundTracks.Add(
                    track.Title,
                    new MediaTrack(
                        soundEffectTrack,
                        track.Loop,
                        track.Volume));
            }
        }

        async Task InitializeSpeechSynthesis()
        {
            await this.aiNarrator.CreateSynthesizedVoicePlaybackList();

            foreach (var track in this.aiNarrator.Speeches)
            {
                MediaPlayer synthTrack = new MediaPlayer
                {
                    AutoPlay = false,
                    IsLoopingEnabled = false,
                    Source = await track.ToPlaybackItem(),
                    Volume = 0.0d
                };

                if (!(String.Equals(track.Title, "ai:Unacceptable", StringComparison.OrdinalIgnoreCase) ||
                    String.Equals(track.Title, "ai:Not_Understand", StringComparison.OrdinalIgnoreCase)))
                {
                    synthTrack.CurrentStateChanged += AudioTrackPlayStateChange;
                }
                this.SoundTracks.Add(
                    track.Title,
                    new MediaTrack(
                        synthTrack,
                        false,
                        0.4));
            }
        }
        
        /// <summary>
        /// Note: This is enabled only for media players that do not have looping enabled.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void AudioTrackPlayStateChange(MediaPlayer sender, object args)
        {
            // If the media item has completed and is paused then call the 
            // playstate change handler method to carry out its logic
            if (sender.PlaybackSession.PlaybackState == MediaPlaybackState.Paused &&
                sender.PlaybackSession.Position == sender.PlaybackSession.NaturalDuration &&
                // this is also a hack
                sender.PlaybackSession.NaturalDuration != new TimeSpan(0))
            {
                // this is a hack as the event gets triggered twice for some reason for the same track.
                sender.PlaybackSession.Position = new TimeSpan(0); 
                HandleAudioTrackEnded(sender, args);
            }
        }

        protected void HandleAudioTrackEnded(MediaPlayer sender, object args)
        {
            AudioTrackCompleted?.Invoke(sender.Source, (EventArgs)args);
        }

        async Task FadeMedia(FadeType type, MediaPlayer player, double baseVolume)
        {
            double fadeTime = 4.0d;
            Stopwatch watch = new Stopwatch();
            watch.Start();
            while (watch.Elapsed < TimeSpan.FromSeconds(fadeTime))
            {
                if (watch.ElapsedMilliseconds % 100 == 0)
                {
                    double tempTime = 0.0d;
                    if (type == FadeType.FadeIn)
                    {
                        tempTime = watch.Elapsed.TotalSeconds;
                    }
                    else if (type == FadeType.FadeOut)
                    {
                        tempTime = fadeTime - watch.Elapsed.TotalSeconds;
                    }
                    player.Volume = (baseVolume * tempTime) / fadeTime;
                }
            }
            watch.Stop();
        }
    }
}
