﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Conscaince.PathSense;
using Conscaince.TrackSense;
using System.Threading;

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
            this.ResetInput();
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

        public Task WaitOnUserInputAsync()
        {
            return Task.Run(() => Loop());
        }

        void Loop()
        {
            while (String.Equals(userInput, "maybe", StringComparison.OrdinalIgnoreCase))
            {
            }
        }

        async Task TraverseNodeTree()
        {
            // TODO: needs to be looped endlessly until the end of the tree is reached.            
            do
            {
                // depict media that is referenced by the current node.
                IList<string> mediaIds = await this.nodeTree.CurrentNode.GetMediaIds();
                foreach (var mediaId in mediaIds)
                {
                    PlayTrack(mediaId);
                };

                // if there are more than one actions to choose from then
                // await user choice before moving to the next node.
                if (this.nodeTree.CurrentNode.Actions.Count > 1)
                {
                    await this.WaitOnUserInputAsync();
                }
                else
                {
                    this.userInput = "n.a";
                }

                this.nodeTree.MoveNext(this.userInput);
                this.ResetInput();

                // determine media which is no longer relevant and remove them.
                IList<string> newMediaIds = await this.nodeTree.CurrentNode.GetMediaIds();
                IList<string> mediaToPause = mediaIds.Except(newMediaIds).ToList();
                foreach (var mediaId in mediaToPause)
                {
                    PauseTrack(mediaId);
                };

            } while (true);

        }
        
        async Task PlayTrack(string mediaId)
        {
            this.audioService.Play(mediaId);
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
