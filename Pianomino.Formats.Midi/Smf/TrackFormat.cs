using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pianomino.Formats.Midi.Smf;

public enum TrackFormat : ushort
{
    Single = 0,
    Simultaneous = 1,
    Independent = 2
}
