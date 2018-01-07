using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;

namespace Conscaince.TrackSense
{
    class JsonReader
    {
        static JsonReader jsonReader;

        public JsonArray BaseTrackArray { get; private set; }
        public JsonArray InterruptTrackArray { get; private set; }

        public static JsonReader JsonReaderInstance
        {
            get
            {
                if (jsonReader == null)
                    jsonReader = new JsonReader();

                return jsonReader;
            }
        }

        public async Task LoadFromApplicationUriAsync(string uriPath)
        {
            await this.LoadFromApplicationUriAsync(new Uri(uriPath));
        }

        async Task LoadFromApplicationUriAsync(Uri uri)
        {
            var storageFile = await StorageFile.GetFileFromApplicationUriAsync(uri);
            var jsonText = await FileIO.ReadTextAsync(storageFile);
            var json = JsonObject.Parse(jsonText);
            json = json.GetNamedObject("playList");

            this.BaseTrackArray = json["base"].GetArray();
            this.InterruptTrackArray = json["interrupt"].GetArray();
        }
    }    
}
