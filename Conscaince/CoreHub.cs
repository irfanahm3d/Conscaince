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

        public async Task Initialize()
        {
            await this.jsonReader.LoadFromApplicationUriAsync(audioListUri);
            await this.audioService.InitializeSoundTracks();
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
    }
}
