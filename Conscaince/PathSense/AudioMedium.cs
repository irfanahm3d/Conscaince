using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;

namespace Conscaince.PathSense
{
    class AudioMedium : AMedium
    {
        public async Task<string> PresentMedium()
        {
            return this.SourceId;
        }
    }
}
