using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conscaince.PathSense
{
    class Node
    {
        public Guid Id { get; private set; }

        public string Title { get; private set; }

        public string MetaDescription { get; private set; }

        public IList<Action> Actions { get; private set; }

        public IList<AMedium> Mediums { get; private set; }

        public Node()
        {
            this.Actions = new List<Action>();
            this.Mediums = new List<AMedium>();
        }

        public async Task PresentMediums()
        {

        }
    }
}
