using Pianomino.Theory;
using System;

namespace Pianomino.Formats.iReal;

public sealed record SongMetadata
{
    public string? Title { get; init; }
    public string? ComposerLastFirstName { get; init; }
    public string? Style { get; init; }
    public DiatonicKey? Key { get; init; }
    public DiatonicKey? ActualKey { get; init; } // irealb:// format only
    public string? ActualStyle { get; init; } // irealb:// format only
    public int? Tempo { get; init; }  // irealb:// format only
    public int? RepeatCount { get; init; } // irealb:// format only

    public static string FlipComposerFirstLastNames(string name)
    {
        int spaceIndex = name.IndexOf(' ');
        if (spaceIndex < 0) return name;
        return name[(spaceIndex + 1)..] + ' ' + name[0..spaceIndex];
    }

    public sealed class CollectSink
    {
        public SongMetadata? Metadata { get; private set; }

        public ChartSinkFactory GetFactory(bool throwIfMultiple)
        {
            IChartSink? CreateSink(SongMetadata metadata, out bool final)
            {
                if (Metadata is null) Metadata = metadata;
                else if (throwIfMultiple) throw new InvalidOperationException();
                final = !throwIfMultiple;
                return null;
            }

            return CreateSink;
        }
    }
}
