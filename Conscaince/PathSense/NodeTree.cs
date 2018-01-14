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

        IDictionary<string, Node> nodes { get; set; }

        Path traversedNodes { get; set; }
        
        public Node CurrentNode { get; private set; }

        public event EventHandler CurrentNodeChanged;
        protected virtual void OnCurrentNodeChanged(EventArgs e)
        {
            CurrentNodeChanged?.Invoke(this.CurrentNode, e);
        }

        public NodeTree()
        {
            this.nodes = new Dictionary<string, Node>();
            this.traversedNodes = new Path();
        }
        
        public async Task GenerateNodeTree()
        {
            JsonArray nodes = jsonReader.NodeArray;

            foreach (var jsonNode in jsonReader.NodeArray)
            {
                Node node = await LoadNode(jsonNode.GetObject());
                this.nodes.Add(node.Id, node);
            }

            // sets the starting node to the current node of the tree structure.
            Node startingNode = null;
            if (this.nodes.TryGetValue("1", out startingNode))
            {
                this.CurrentNode = startingNode;
                OnCurrentNodeChanged(new EventArgs());
            }
        }

        public async Task MoveNext(string actionChoice)
        {
            // checks what action has been selected to move to the next node
            Action action = 
                this.CurrentNode.Actions.Where(
                    a => String.Equals(
                        a.Choice, actionChoice, StringComparison.OrdinalIgnoreCase))
                        .FirstOrDefault();

            if (action == null)
            {
                throw new ArgumentNullException("action is null");
            }

            Node nextNode = null;
            if (!this.nodes.TryGetValue(action.NextNodeId[0], out nextNode))
            {
                throw new Exception("this was not meant to happen");
            }

            this.CurrentNode = nextNode;
            OnCurrentNodeChanged(new EventArgs());
        }

        async Task<Node> LoadNode(JsonObject json)
        {
            var node = new Node();
            await node.PopulateNode(json);
            return node;
        }
    }
}
