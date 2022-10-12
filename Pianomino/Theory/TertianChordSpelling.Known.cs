using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pianomino.Theory;

using Alterations = ChordDegreeAlterationMask;

partial struct TertianChordSpelling
{
    public static TertianChordSpelling Minor => new(MajorOrMinor.Minor, Alterations.Natural);
    public static TertianChordSpelling Major => new(MajorOrMinor.Major, Alterations.Natural);
    public static TertianChordSpelling Diminished => new(MajorOrMinor.Minor, Alterations.Flat);
    public static TertianChordSpelling Augmented => new(MajorOrMinor.Major, Alterations.Sharp);

    public static TertianChordSpelling Minor7th => new(MajorOrMinor.Minor, Alterations.Natural, Alterations.Flat);
    public static TertianChordSpelling Minor7thFlat5 => new(MajorOrMinor.Minor, Alterations.Flat, Alterations.Flat);
    public static TertianChordSpelling HalfDiminished => Minor7thFlat5;
    public static TertianChordSpelling Diminished7th => new(MajorOrMinor.Minor, Alterations.Flat, Alterations.DoubleFlat);
    public static TertianChordSpelling Dominant7th => new(MajorOrMinor.Major, Alterations.Natural, Alterations.Flat);
    public static TertianChordSpelling Major7th => new(MajorOrMinor.Major, Alterations.Natural, Alterations.Natural);
    public static TertianChordSpelling MinorMajor7th => new(MajorOrMinor.Minor, Alterations.Natural, Alterations.Natural);
    public static TertianChordSpelling Augmented7th => new(MajorOrMinor.Major, Alterations.Sharp, Alterations.Flat);
    public static TertianChordSpelling AugmentedMajor7th => new(MajorOrMinor.Major, Alterations.Sharp, Alterations.Natural);

    public static TertianChordSpelling Minor9th => new(MajorOrMinor.Minor, Alterations.Natural, Alterations.Flat, Alterations.Natural);
    public static TertianChordSpelling Major9th => new(MajorOrMinor.Major, Alterations.Natural, Alterations.Natural, Alterations.Natural);
    public static TertianChordSpelling Dominant9th => new(MajorOrMinor.Major, Alterations.Natural, Alterations.Flat, Alterations.Natural);
    public static TertianChordSpelling Dominant7thFlat9 => new(MajorOrMinor.Major, Alterations.Natural, Alterations.Flat, Alterations.Flat);
    public static TertianChordSpelling Dominant9thSharp9 => new(MajorOrMinor.Major, Alterations.Natural, Alterations.Flat, Alterations.Sharp);

    public static TertianChordSpelling Major6th => new(MajorOrMinor.Major, Alterations.Natural, thirteenths: Alterations.Natural);
    public static TertianChordSpelling Minor6th => new(MajorOrMinor.Minor, Alterations.Natural, thirteenths: Alterations.Natural);

    public static TertianChordSpelling Major69 => new(MajorOrMinor.Major, Alterations.Natural, ninths: Alterations.Natural, thirteenths: Alterations.Natural);
    public static TertianChordSpelling Minor69 => new(MajorOrMinor.Minor, Alterations.Natural, ninths: Alterations.Natural, thirteenths: Alterations.Natural);

}
