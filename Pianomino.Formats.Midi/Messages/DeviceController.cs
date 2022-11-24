using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pianomino.Formats.Midi.Messages;

public enum DeviceController : byte
{
    MasterVolume = 1,
    MasterBalance = 2
}
