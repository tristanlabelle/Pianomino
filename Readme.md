# Pianomino

.NET libraries for manipulating concepts and formats of Western music.

Pianomino follows C# and .NET best practices: 
- Immutable value types with compact, non-redundant data representations
- Rich structs and enums over primitive types
- Tasteful use of extension methods and operator overloading

## Pianomino.Theory

Basic types for concepts of Western music theory in 12-tone equal temperament.

### Note & interval types

`Pianomino.Theory`'s representation of musical notes and intervals is split into three categories:

- **Chromatic**, which does not distinguish between `C#3` and `Db3`, like piano keys. Here, an interval is the distance between two chromatic notes, a number of semitones.
- **Diatonic**, which can only represent natural notes, such as `C3` and `D3` but nothing in-between. Here, an interval is the distance between two diatonic notes, a number of diatonic steps.
- "**Full**", which is our usual understanding of a note, where `C#3` and `Db3` are distinct. Here, an interval is our usual understanding of an interval, a distance between two full notes, which distinguishes an `augmented fourth` from a `diminished fifth`.

One observation is that the "full" representation is a tuple of the diatonic and the chromatic representations. This allows simple arithmetic operations between notes such as `(C3 + perfect fifth) - B3 = major third`. In this arithmetic system, notes and intervals are analogous to points and vectors, and descending intervals can be represented.

In addition, `Pianomino.Theory` includes octave-aware and octave-agnostic types. Octave-agnostic types are used when octave information is meaningless, such as for chords or keys. The full matrix of types is as follows:

|                            | Notes          | Intervals       |
| -------------------------- | -------------- | --------------- |
| Full, octave-agnostic      | NoteClass      | IntervalClass   |
| Full, octave-aware         | NotePitch      | Interval        |
| Diatonic, octave-agnostic  | NoteLetter     | DiatonicDegree  |
| Diatonic, octave-aware     | NaturalPitch   | int             |
| Chromatic, octave-agnostic | ChromaticClass | ChromaticDegree |
| Chromatic, octave-aware    | ChromaticPitch | int             |

## Pianomino.Formats.Smufl

Provides types representing Standard Music Font Layout (SMuFL) concepts and file format I/O.

## Pianomino.Formats.iReal

Supports I/O of chart formats from the iReal Pro application.
