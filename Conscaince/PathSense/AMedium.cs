using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Conscaince.PathSense
{
    abstract class AMedium
    {
        public Guid Id { get; private set; }

        public string MetaDescription { get; private set; }

        public string SourceId { get; private set; }

        /// <summary>
        /// Determines if the medium is tied to a particular node
        /// or traverses across multiple nodes.
        /// </summary>
        public bool IsTraversing { get; private set; }

        /// <summary>
        /// Displays/plays the asset from the source uri.
        /// </summary>
        /// <returns>Void</returns>
        public async Task PresentMedium()
        {
        }
    }
}
