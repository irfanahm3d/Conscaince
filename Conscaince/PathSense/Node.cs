using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace Conscaince.PathSense
{
    class Node
    {
        public string Id { get; private set; }

        public string Title { get; private set; }

        public string MetaDescription { get; private set; }

        public IList<Action> Actions { get; private set; }

        public IList<AMedium> Media { get; private set; }

        public async Task PopulateNode(JsonObject json)
        {
            this.Id = json.GetNamedString("id", string.Empty);
            this.Title = json.GetNamedString("title", string.Empty);
            this.MetaDescription = json.GetNamedString("metaDescription", string.Empty);

            this.Actions = new List<Action>();
            var jsonActions = json.GetNamedArray("actions");
            if (jsonActions.Count != 0)
            {
                foreach (var jsonAction in jsonActions)
                {
                    this.Actions.Add(await LoadAction(jsonAction.GetObject()));
                };
            }

            this.Media = new List<AMedium>();
            var jsonMedia = json.GetNamedArray("media");
            if (jsonMedia.Count != 0)
            {
                foreach (var jsonMedium in jsonMedia)
                {
                    this.Media.Add(await LoadMedium(jsonMedium.GetObject()));
                }
            }
        }

        async Task<Action> LoadAction(JsonObject json)
        {
            var action = new Action();
            await action.PopulateAction(json);
            return action;
        }

        async Task<AMedium> LoadMedium(JsonObject json)
        {
            AMedium medium = null;
            string mediumType = json.GetNamedString("type", string.Empty);

            if (String.IsNullOrEmpty(mediumType))
            {
                throw new Exception("This should not be empty");
            }

            if (String.Equals(mediumType, "audio", StringComparison.OrdinalIgnoreCase))
            {
                medium = new AudioMedium();
                await medium.PopulateMedium(json);
            }

            return medium;
        }

        public async Task PresentMediums()
        {

        }
    }
}
