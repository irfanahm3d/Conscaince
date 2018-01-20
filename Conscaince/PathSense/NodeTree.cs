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
        int currentNodeCompleteCount { get; set; }

        public event EventHandler CurrentNodeChanged;
        protected virtual void OnCurrentNodeChanged(EventArgs e)
        {
            this.currentNodeCompleteCount = 0;
            CurrentNodeChanged?.Invoke(this.CurrentNode, e);
        }

        public NodeTree()
        {
            this.nodes = new Dictionary<string, Node>();
            this.traversedNodes = new Path();
            this.currentNodeCompleteCount = 0;
        }

        public async Task DetermineNodeCompletion(bool hasTrackCompleted)
        {
            if (hasTrackCompleted)
            {
                this.currentNodeCompleteCount += 1;
            }

            if (this.currentNodeCompleteCount == this.CurrentNode.NonTraversingMediaCount)
            {
                this.CurrentNode.IsCompleted = true;
            }

            if (this.currentNodeCompleteCount > this.CurrentNode.NonTraversingMediaCount)
            {
                throw new ArgumentException("this should not happen");
            }
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

        public async Task<IList<Node>> SeekNext()
        {
            IList<Node> nextNodes = new List<Node>();
            foreach (var action in this.CurrentNode.Actions)
            {
                // can take the 0th index since currently one action points only
                // to one node. THIS IS A HACK. Will need to change
                Node node;
                if (this.nodes.TryGetValue(action.NextNodeId[0], out node))
                {
                    nextNodes.Add(node);
                }
            }

            return nextNodes;
        }

        public async Task<bool> MoveNext(string actionChoice)
        {
            bool isNext = true;
            // checks what action has been selected to move to the next node
            Action action = 
                this.CurrentNode.Actions.Where(
                    a => String.Equals(
                        a.Choice, actionChoice, StringComparison.OrdinalIgnoreCase))
                        .FirstOrDefault();

            if (action == null)
            {
                isNext = false;
            }

            Node nextNode = null;
            if (!this.nodes.TryGetValue(action.NextNodeId[0], out nextNode))
            {
                throw new Exception("this was not meant to happen");
            }

            this.CurrentNode = nextNode;
            OnCurrentNodeChanged(new EventArgs());

            return isNext;
        }

        async Task<Node> LoadNode(JsonObject json)
        {
            var node = new Node();
            await node.PopulateNode(json);
            return node;
        }
    }
}
