using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace Conscaince.PathSense
{
    abstract class AMedium
    {
        public string Id { get; private set; }

        public string MetaDescription { get; private set; }

        public string SourceId { get; private set; }

        /// <summary>
        /// Determines if the medium is tied to a particular node
        /// or traverses across multiple nodes.
        /// </summary>
        public bool IsTraversing { get; private set; }

        public async Task PopulateMedium(JsonObject json)
        {
            this.Id = json.GetNamedString("id", string.Empty);
            this.MetaDescription = json.GetNamedString("metaDescription", string.Empty);
            this.SourceId = json.GetNamedString("sourceId", string.Empty);
            this.IsTraversing = json.GetNamedBoolean("isTraversing", false);
        }

        /// <summary>
        /// Displays/plays the asset from the source uri.
        /// </summary>
        /// <returns>Void</returns>
        public async Task PresentMedium()
        {
        }
    }
}
