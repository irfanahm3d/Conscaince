using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace Conscaince.PathSense
{
    /// <summary>
    /// This class handles the generation of the node tree structure
    /// from the json file. 
    /// </summary>
    class NodeTree
    {
        JsonReader jsonReader = JsonReader.JsonReaderInstance;

        static NodeTree nodeTreeInstance;

        public static NodeTree NodeTreeInstance
        {
            get
            {
                if (nodeTreeInstance == null)
                    nodeTreeInstance = new NodeTree();

                return nodeTreeInstance;
            }
        }

        public IList<Node> Nodes { get; private set; }

        public NodeTree()
        {
            this.Nodes = new List<Node>();
        }
        
        public async Task GenerateNodeTree()
        {
            JsonArray nodes = jsonReader.NodeArray;

            foreach (var jsonNode in jsonReader.NodeArray)
            {
                this.Nodes.Add(await LoadNode(jsonNode.GetObject()));
            }
        }

        public async Task StartTraversal()
        {

        }

        async Task<Node> LoadNode(JsonObject json)
        {
            var node = new Node();
            await node.PopulateNode(json);
            return node;
        }
    }
}
