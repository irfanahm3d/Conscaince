using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Conscaince.PathSense;
using Conscaince.TrackSense;
using System.Threading;
using System.Diagnostics;
using Windows.ApplicationModel.Core;

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
        const string aiSpeechListUri = "ms-appx:///Assets/conscainceSpeechList.json";
        const string nodeListUri = "ms-appx:///Assets/nodelist.json";

        /// <summary>
        /// The audio player service instance.
        /// </summary>
        AudioPlayerService audioService = AudioPlayerService.AudioPlayerInstance;

        JsonReader jsonReader = JsonReader.JsonReaderInstance;

        NodeTree nodeTree = NodeTree.NodeTreeInstance;

        UserInput input = UserInput.UserInputInstance;

        string currentNodeTitle;

        public event EventHandler CurrentNodeChanged;
        public event EventHandler CurrentNodeCompleted;

        protected virtual void OnCurrentNodeCompleted(EventArgs e)
        {
            CurrentNodeCompleted?.Invoke(this.nodeTree.CurrentNode.Actions.Count(), e);
        }

        protected virtual void InvokeCurrentNodeChanged(EventArgs e)
        {
            CurrentNodeChanged?.Invoke(this.currentNodeTitle, e);
        }

        void OnCurrentNodeChanged(object sender, EventArgs e)
        {
            currentNodeTitle = ((Node)sender).Title;
            InvokeCurrentNodeChanged(new EventArgs());
        }

        public string userInput { get; set; }

        public async Task Initialize()
        {
            ResetInput();
            await this.jsonReader.LoadFromApplicationUriAsync(audioListUri);
            await this.jsonReader.LoadFromApplicationUriAsync(aiSpeechListUri);
            this.audioService.InitializeSoundTracks();
            this.audioService.AudioTrackCompleted += HasNodeCompleted;


            // load nodes after all media have been loaded and initialized
            // from the json files
            await this.jsonReader.LoadFromApplicationUriAsync(nodeListUri);
            this.nodeTree.CurrentNodeChanged += OnCurrentNodeChanged;
            this.nodeTree.GenerateNodeTree();
        }
        
        void ResetInput()
        {
            this.userInput = string.Empty;
        }

        public async Task DisposeMedia()
        {
            this.audioService.Dispose();
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
                this.userInput = string.Empty;
                await WaitOnUserInputAsync();
            }
            else
            {
                this.userInput = "n.a";
            }
        }

        Task WaitOnUserInputAsync()
        {
            return Task.Run(async () =>
            {
                while (true)
                {
                    //userInput = await input.RecordSpeechFromMicrophoneAsync();

                    if (String.IsNullOrEmpty(this.userInput))
                    {
                        // i did not quite hear that
                        //PlayTrack("ai:Not_Understand", false);
                    }
                    else if (String.Equals(this.userInput, "maybe", StringComparison.OrdinalIgnoreCase))
                    {
                        // this is an unacceptable response
                        //PlayTrack("ai:Unacceptable", false);
                    }
                    else
                    {
                        break;
                    }
                }
            });
        }

        async Task TraverseNodeTree()
        {
            bool isNext = true;
            // TODO: needs to be looped endlessly until the end of the tree is reached.            
            do
            {
                IList<AMedium> mediaList = await this.nodeTree.CurrentNode.GetMedia();
                PlayTracks(mediaList);

                await WaitNodeCompletion();
                await GetUserInput();
                isNext = await this.nodeTree.MoveNext(this.userInput);
                ResetInput();
                
                PauseTracks(mediaList);
            } while (isNext);

            DisposeMedia();

            CoreApplication.Exit();
        }

        async Task PlayTracks(IList<AMedium> mediaList)
        {
            IEnumerable<AMedium> zeroTimeMedia =
                mediaList.Where(m => m.RelativeStartTime == new TimeSpan(0, 0, 0));
            Task.Run(async () =>
            {
                // immediately plays media that is to be started at 0th time, i.e.
                // the time relative to the processing of the node media.
                Parallel.ForEach<AMedium>(zeroTimeMedia, media => { PlayTrack(media.SourceId, media.IsTraversing); });
            });

            Task.Run(async () =>
            {
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
            });
        }

        async Task PauseTracks(IList<AMedium> mediaList)
        {
            Task.Run(async () =>
            {
                // determine media which is no longer relevant and remove them.
                IEnumerable<AMedium> newMediaList = await this.nodeTree.CurrentNode.GetMedia();
                IEnumerable<AMedium> mediaToPause = mediaList.Except(newMediaList, new MediumComparer());
                Parallel.ForEach<AMedium>(mediaToPause, media => { PauseTrack(media.SourceId); });
            });
        }

        async Task PlayTrack(string mediaId, bool loop)
        {
            this.audioService.Play(mediaId, loop);
        }

        async Task PauseTrack(string mediaId)
        {
            this.audioService.Pause(mediaId);
        }

        async Task ControlTrackVolume()
        {
        }

        Task WaitNodeCompletion()
        {
            return Task.Run(() =>
            {
                while (!this.nodeTree.CurrentNode.IsCompleted) { }
                OnCurrentNodeCompleted(new EventArgs());
            });
        }

        void HasNodeCompleted(object sender, EventArgs e)
        {
            this.nodeTree.DetermineNodeCompletion(true);
        }
    }
}
