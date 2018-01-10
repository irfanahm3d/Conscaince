using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conscaince.PathSense
{
    /// <summary>
    /// Contains the list of nodes that have been traversed by the user
    /// </summary>
    class Path
    {
        public IList<Node> UserPath { get; private set; }

        // can be found by the last node the path is on... this might not be necessary.
        public Node CurrentNode { get; private set; }

        public Path()
        {
            this.UserPath = new List<Node>();
        }
    }
}
