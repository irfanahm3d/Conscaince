﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Conscaince.PathSense;
using Conscaince.TrackSense;
using System.Threading;
using System.Diagnostics;

namespace Conscaince
{
    class CoreHub
    {
        static CoreHub coreHubInstance;

        public static CoreHub CoreHubInstance
        {
            get
            {
                if (coreHubInstance == null)
                    coreHubInstance = new CoreHub();

                return coreHubInstance;
            }
        }

        const string audioListUri = "ms-appx:///Assets/audiolist.json";
        const string nodeListUri = "ms-appx:///Assets/nodelist.json";

        /// <summary>
        /// The audio player service instance.
        /// </summary>
        AudioPlayerService audioService = AudioPlayerService.AudioPlayerInstance;

        JsonReader jsonReader = JsonReader.JsonReaderInstance;

        NodeTree nodeTree = NodeTree.NodeTreeInstance;

        string currentNodeTitle;

        public event EventHandler CurrentNodeChanged;
        protected virtual void ReadyPlayerOne(EventArgs e)
        {
            CurrentNodeChanged?.Invoke(this.currentNodeTitle, e);
        }

        void OnCurrentNodeChanged(object sender, EventArgs e)
        {
            currentNodeTitle = ((Node)sender).Title;
            ReadyPlayerOne(new EventArgs());
        }

        public string userInput { get; set; }

        public async Task Initialize()
        {
            ResetInput();
            await this.jsonReader.LoadFromApplicationUriAsync(audioListUri);
            this.audioService.InitializeSoundTracks();

            // load nodes after all media have been loaded and initialized
            // from the json files
            await this.jsonReader.LoadFromApplicationUriAsync(nodeListUri);
            this.nodeTree.CurrentNodeChanged += OnCurrentNodeChanged;
            this.nodeTree.GenerateNodeTree();
        }
        
        void ResetInput()
        {
            this.userInput = "maybe";
        }
                
        public async Task BufferMedia(int depth)
        {

        }

        public async Task DisposeUnusedMedia()
        {

        }

        public async Task<string> GetSourceId()
        {
            return string.Empty;
        }

        public async Task TraverseNodesAsync()
        {
            await Task.Run(() => TraverseNodeTree());
        }

        async Task GetUserInput()
        {
            // if there are more than one actions to choose from then
            // await user choice before moving to the next node.
            if (this.nodeTree.CurrentNode.Actions.Count > 1)
            {
                await WaitOnUserInputAsync();
            }
            else
            {
                this.userInput = "n.a";
            }
        }

        Task WaitOnUserInputAsync()
        {
            return Task.Run(() => 
            {
                while (String.Equals(
                    userInput,
                    "maybe",
                    StringComparison.OrdinalIgnoreCase))
                {
                }
            });
        }

        async Task TraverseNodeTree()
        {
            // TODO: needs to be looped endlessly until the end of the tree is reached.            
            do
            {
                IList<AMedium> mediaList = await this.nodeTree.CurrentNode.GetMedia();
                PlayTracks(mediaList);

                await GetUserInput();
                this.nodeTree.MoveNext(this.userInput);
                ResetInput();
                
                PauseTracks(mediaList);
            } while (true);

        }

        async Task PlayTracks(IList<AMedium> mediaList)
        {
            IEnumerable<AMedium> zeroTimeMedia = 
                mediaList.Where(m => m.RelativeStartTime == new TimeSpan(0, 0, 0));

            // immediately plays media that is to be started at 0th time, i.e.
            // the time relative to the processing of the node media.
            foreach (var media in zeroTimeMedia)
            {
                PlayTrack(media.SourceId, media.IsTraversing);
            };

            // for media that is to be played after 0th time put them in ascending order
            // and play them according to the time marked.
            IList<AMedium> ascendingTimeMedia = 
                mediaList.Except(zeroTimeMedia).OrderBy(m => m.RelativeStartTime).ToList();
            
            Stopwatch watch = new Stopwatch();
            watch.Start();
            while (ascendingTimeMedia.Count() != 0)
            {
                AMedium relativeMedia = ascendingTimeMedia.First();
                if (watch.Elapsed >= relativeMedia.RelativeStartTime)
                {
                    PlayTrack(relativeMedia.SourceId, relativeMedia.IsTraversing);
                    ascendingTimeMedia.RemoveAt(0);
                }
            }
            watch.Stop();
        }

        async Task PauseTracks(IList<AMedium> mediaList)
        {
            // determine media which is no longer relevant and remove them.
            IEnumerable<AMedium> newMediaList = await this.nodeTree.CurrentNode.GetMedia();
            IEnumerable<AMedium> mediaToPause = mediaList.Except(newMediaList, new MediumComparer());
            foreach (var media in mediaToPause)
            {
                PauseTrack(media.SourceId);
            };
        }
        
        async Task PlayTrack(string mediaId, bool loop)
        {
            this.audioService.Play(mediaId, loop);
        }

        void WaitForTrack()
        {
        }

        async Task PauseTrack(string mediaId)
        {
            this.audioService.Pause(mediaId);
        }

        async Task ControlTrackVolume()
        {
        }
    }
}