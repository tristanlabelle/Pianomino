using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pianomino.Formats.Midi.Smf;

public enum FileValidationError
{
    InvalidMThdFormat,
    InvalidMThdTimeDivision,
    MultipleMThdChunks,
    ExtraTracks,
    InvalidTrackForMetaEvent,
    UnorderedEvents,
    InvalidMessageData,
    RunningStatusInvalidated,
    UnterminatedSysEx,
    InvalidTrackTermination
}

public delegate void FileValidationErrorHandler(FileValidationError error);
