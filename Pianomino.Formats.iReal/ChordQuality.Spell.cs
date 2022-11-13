using Pianomino.Theory;
using System;

namespace Pianomino.Formats.iReal;

partial struct ChordQuality
{
    public TertianChordSpelling Spell()
    {
        MajorOrMinor? third = MajorOrMinor.Major;
        var fifths = ChordDegreeAlterationMask.Natural;
        var sevenths = ChordDegreeAlterationMask.None;
        var ninths = ChordDegreeAlterationMask.None;
        var elevenths = ChordDegreeAlterationMask.None;
        var thirteenths = ChordDegreeAlterationMask.None;
        switch (BaseQuality)
        {
            case ChordBaseQuality.ImplicitMajor:
            case ChordBaseQuality.ExplicitMajor:
                break;

            case ChordBaseQuality.Minor:
                third = MajorOrMinor.Minor;
                break;

            case ChordBaseQuality.Diminished:
                third = MajorOrMinor.Minor;
                fifths = ChordDegreeAlterationMask.Flat;
                break;

            case ChordBaseQuality.Augmented:
                fifths = ChordDegreeAlterationMask.Sharp;
                break;

            case ChordBaseQuality.HalfDiminished:
                third = MajorOrMinor.Minor;
                fifths = ChordDegreeAlterationMask.Flat;
                sevenths = ChordDegreeAlterationMask.Flat;
                break;

            case ChordBaseQuality.Suspended2:
                third = null;
                ninths = ChordDegreeAlterationMask.Natural;
                break;

            case ChordBaseQuality.Suspended4:
                third = null;
                elevenths = ChordDegreeAlterationMask.Natural;
                break;

            case ChordBaseQuality.Power:
                third = null;
                break;

            case ChordBaseQuality.Altered:
                ninths = ChordDegreeAlterationMask.Flat | ChordDegreeAlterationMask.Sharp;
                fifths = ChordDegreeAlterationMask.Flat | ChordDegreeAlterationMask.Sharp;
                sevenths = ChordDegreeAlterationMask.Flat;
                break;

            default: throw new UnreachableException();
        }

        if (Extension is JazzChordExtension extension)
        {
            sevenths = extension.GetSeventhAlterationMask(BaseQuality == ChordBaseQuality.Diminished);
            ninths = extension.NinthAlterationMask;
            if (extension.Degree == TertianChordDegree.Eleventh)
            {
                elevenths = ChordDegreeAlterationMask.Natural;
                third = null; // Usually omitted in 11th chords
            }
            else if (extension.Degree == TertianChordDegree.Thirteenth)
            {
                thirteenths = ChordDegreeAlterationMask.Natural;
                // 11th usually omitted in 13th chords
            }
        }

        foreach (var tweak in Tweaks)
        {
            var degree = TertianChordDegreeEnum.FromNumber(tweak.DegreeNumber);
            if (tweak.Type == ChordTweakType.Add)
            {
                if (degree == TertianChordDegree.Ninth)
                    ninths = ChordDegreeAlterationMask.Natural;
                else if (degree == TertianChordDegree.Eleventh)
                    elevenths |= ChordDegreeAlterationMask.Natural;
                else if (degree == TertianChordDegree.Thirteenth)
                    thirteenths |= ChordDegreeAlterationMask.Natural;
                else if (degree == TertianChordDegree.Third)
                {
                    // Weird susadd3 construct
                    if (third is not null) throw new FormatException();
                    third = MajorOrMinor.Major;
                }
                else
                    throw new NotImplementedException();
            }
            else if (tweak.Type == ChordTweakType.Flat)
            {
                if (degree == TertianChordDegree.Fifth)
                    fifths = (fifths & ~ChordDegreeAlterationMask.Natural) | ChordDegreeAlterationMask.Flat;
                else if (degree == TertianChordDegree.Ninth)
                    ninths |= ChordDegreeAlterationMask.Flat;
                else if (degree == TertianChordDegree.Thirteenth)
                    thirteenths = ChordDegreeAlterationMask.Flat;
                else
                    throw new NotImplementedException();
            }
            else if (tweak.Type == ChordTweakType.Sharp)
            {
                if (degree == TertianChordDegree.Fifth)
                    fifths = (fifths & ~ChordDegreeAlterationMask.Natural) | ChordDegreeAlterationMask.Sharp;
                else if (degree == TertianChordDegree.Ninth)
                    ninths |= ChordDegreeAlterationMask.Sharp;
                else if (degree == TertianChordDegree.Eleventh)
                    elevenths = ChordDegreeAlterationMask.Sharp;
                else if (degree == TertianChordDegree.Thirteenth)
                    thirteenths = ChordDegreeAlterationMask.Sharp;
                else
                    throw new NotImplementedException();
            }
        }

        return new(third, fifths, sevenths, ninths, elevenths, thirteenths);
    }
}
