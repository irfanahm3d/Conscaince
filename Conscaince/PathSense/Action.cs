using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace Conscaince.PathSense
{
    class Action
    {
        public string Choice { get; private set; }

        public IList<string> NextNodeId { get; private set; }

        public async Task PopulateAction(JsonObject json)
        {
            this.Choice = json.GetNamedString("choice", string.Empty);
            this.NextNodeId = new List<string>();
            var jsonNodeIdArray = json["nextNodeId"].GetArray();
            foreach (var nodeId in jsonNodeIdArray)
            {
                this.NextNodeId.Add(nodeId.GetString());
            }
        }
    }
}
