using System;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;

namespace Conscaince
{
    class JsonReader
    {
        static JsonReader jsonReader;

        public static JsonReader JsonReaderInstance
        {
            get
            {
                if (jsonReader == null)
                    jsonReader = new JsonReader();

                return jsonReader;
            }
        }

        public JsonArray BaseTrackArray { get; private set; }
        public JsonArray NodeArray { get; private set; }
        public JsonArray SpeechArray { get; private set; }

        public async Task LoadFromApplicationUriAsync(string uriPath)
        {
            if (uriPath.IndexOf("audio", StringComparison.OrdinalIgnoreCase) >=0 )
            {
                await this.LoadAudioList(new Uri(uriPath));
            }
            else if (uriPath.IndexOf("node", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                await this.LoadNodeList(new Uri(uriPath));
            }
            else if (uriPath.IndexOf("speech", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                await this.LoadAiSpeechList(new Uri(uriPath));
            }
        }

        async Task<JsonObject> LoadJson(Uri uri)
        {
            var storageFile = await StorageFile.GetFileFromApplicationUriAsync(uri);
            var jsonText = await FileIO.ReadTextAsync(storageFile);
            return JsonObject.Parse(jsonText);
        }

        async Task LoadAudioList(Uri uri)
        {
            JsonObject json = await LoadJson(uri);
            json = json.GetNamedObject("audioList");

            this.BaseTrackArray = json["base"].GetArray();
        }

        async Task LoadNodeList(Uri uri)
        {
            JsonObject json = await LoadJson(uri);
            json = json.GetNamedObject("nodeList");

            this.NodeArray = json["nodes"].GetArray();
        }

        async Task LoadAiSpeechList(Uri uri)
        {
            JsonObject json = await LoadJson(uri);
            json = json.GetNamedObject("aiSpeechList");

            this.SpeechArray = json["dialogue"].GetArray();
        }
    }    
}
