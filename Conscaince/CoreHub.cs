using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Conscaince.PathSense;
using Conscaince.TrackSense;

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

        public async Task Initialize()
        {
            await this.jsonReader.LoadFromApplicationUriAsync(audioListUri);
            await this.audioService.InitializeSoundTracks();

            // load nodes after all media have been loaded and initialized
            // from the json files
            await this.jsonReader.LoadFromApplicationUriAsync(nodeListUri);
            await this.nodeTree.GenerateNodeTree();
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

        public async Task BeginNodeTraversal()
        {
            // TODO: needs to be looped endlessly until the end of the tree is reached.

            // plays the media
            do
            {
                // depict media that is referenced by the current node.
                IList<string> mediaIds = await this.nodeTree.CurrentNode.GetMediaIds();
                foreach (var mediaId in mediaIds)
                {
                    await PlayTrack(mediaId);
                };

                await this.nodeTree.MoveNext("no");

                // determine media which is no longer relevant and remove them.
                IList<string> newMediaIds = await this.nodeTree.CurrentNode.GetMediaIds();
                IList<string> mediaToPause = mediaIds.Except(newMediaIds).ToList();
                foreach (var mediaId in mediaToPause)
                {
                    await PauseTrack(mediaId);
                };

            } while (true);

        }


        async Task PlayTrack(string mediaId)
        {
            await this.audioService.Play(mediaId);
        }

        async Task PauseTrack(string mediaId)
        {
            await this.audioService.Pause(mediaId);
        }

        async Task ControlTrackVolume()
        {
        }
    }
}
