using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rnet.Profiles.Media.Audio
{
    [ProfileContract("media.audio", "Sink")]
    public interface ISink
    {
        int SourceId { get; }
    }
}
