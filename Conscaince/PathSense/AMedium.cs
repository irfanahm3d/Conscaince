using System;
using System.Collections.Generic;
using System.Globalization;
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

        public TimeSpan RelativeStartTime { get; private set; }

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

            var jsonTime = json.GetNamedString("relativeStartTime", string.Empty);
            this.RelativeStartTime = new TimeSpan(0, 0, 0);
            if (!String.IsNullOrEmpty(jsonTime))
            {
                TimeSpan tempTime;
                if (TimeSpan.TryParseExact(
                        jsonTime,
                        "c",
                        CultureInfo.InvariantCulture,
                        out tempTime))
                {
                    this.RelativeStartTime = tempTime;
                }
            }
        }

        /// <summary>
        /// Displays/plays the asset from the source uri.
        /// </summary>
        /// <returns>Void</returns>
        public async Task PresentMedium()
        {
        }
    }

    class MediumComparer : IEqualityComparer<AMedium>
    {
        public bool Equals(AMedium x, AMedium y)
        {
            return String.Equals(
                x.SourceId,
                y.SourceId,
                StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode(AMedium obj)
        {
            int hashCode = 0;
            hashCode += obj.SourceId.GetHashCode();
            return hashCode;
        }
    }
}
