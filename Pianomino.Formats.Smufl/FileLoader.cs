using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Pianomino.Formats.Smufl;

public readonly struct FontFileInfo
{
    public string FacePath { get; }
    public string MetadataPath { get; }
    public string Name => Path.GetFileNameWithoutExtension(MetadataPath);

    internal FontFileInfo(string facePath, string metadataPath)
        => (FacePath, MetadataPath) = (facePath, metadataPath);
}

/// <summary>
/// Provides methods for parsing the various SMuFL metadata files.
/// </summary>
public static class FileLoader
{
    #region Files
    private static readonly string[] FaceFileExtensionsWithDot = new[] { ".odf", ".ttf" };
    private static string UserFontMetadataDirPath => Environment.ExpandEnvironmentVariables(@"%LOCALAPPDATA%\SMuFL\Fonts");
    private static string SystemFontMetadataDirPath => Environment.ExpandEnvironmentVariables(@"%COMMONPROGRAMFILES%\SMuFL\Fonts");

    private static IEnumerable<FontFileInfo> EnumerateFonts(string metadataDirectoryPath, bool allowLocalFaceFile)
    {
        var fontsFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
        foreach (var metadataPath in Directory.EnumerateFiles(metadataDirectoryPath, "*.json", SearchOption.AllDirectories))
        {
            string fontName = Path.GetFileName(metadataPath);
            string faceFileSearchPattern = fontName + ".*";

            if (allowLocalFaceFile)
            {
                string metadataDirPath = Path.GetDirectoryName(metadataPath)!; // By EnumerateFiles, won't be null
                string? localFacePath = Directory.EnumerateFiles(metadataDirPath, faceFileSearchPattern)
                    .FirstOrDefault(p => FaceFileExtensionsWithDot.Contains(Path.GetExtension(p)));
                if (localFacePath != null)
                {
                    yield return new FontFileInfo(localFacePath, metadataPath);
                    continue;
                }
            }

            string? facePath = Directory.EnumerateFiles(fontsFolderPath, faceFileSearchPattern)
                .FirstOrDefault(p => FaceFileExtensionsWithDot.Contains(Path.GetExtension(p)));
            if (facePath != null) yield return new FontFileInfo(facePath, metadataPath);
        }
    }

    public static IEnumerable<FontFileInfo> EnumerateFonts(string? localMetadataDirectoryPath = null)
    {
        if (localMetadataDirectoryPath != null)
        {
            foreach (var font in EnumerateFonts(localMetadataDirectoryPath, allowLocalFaceFile: true))
                yield return font;
        }

        foreach (var font in EnumerateFonts(UserFontMetadataDirPath, allowLocalFaceFile: false))
            yield return font;

        foreach (var font in EnumerateFonts(SystemFontMetadataDirPath, allowLocalFaceFile: false))
            yield return font;
    }

    public static string? TryFindFontMetadataFilePath(string fontName, string? localDirectoryPath = null)
    {
        var fileName = fontName + ".json";

        string path;
        if (localDirectoryPath != null)
        {
            path = Path.Combine(localDirectoryPath, fileName);
            if (File.Exists(path)) return Path.GetFullPath(path);
        }

        path = $@"{UserFontMetadataDirPath}\{fontName}\{fileName}";
        if (File.Exists(path)) return Path.GetFullPath(path);

        path = $@"{SystemFontMetadataDirPath}\{fontName}\{fileName}";
        if (File.Exists(path)) return Path.GetFullPath(path);

        return null;
    }
    #endregion

    #region Metadata loading
    public static void FeedFontMetadata(Stream stream, IFontMetadataSink sink,
        FontMetadataMask mask = FontMetadataMask.All)
    {
        if (!stream.CanRead) throw new ArgumentException();

        FeedFontMetadata(new StreamReader(stream), sink, mask);
    }

    public static void FeedFontMetadata(TextReader textReader, IFontMetadataSink sink,
        FontMetadataMask mask = FontMetadataMask.All)
    {
        var document = JObject.Load(new JsonTextReader(textReader));

        if ((mask & FontMetadataMask.Name) != 0)
            sink.SetName((string)document["fontName"]);

        if ((mask & FontMetadataMask.Version) != 0)
            sink.SetVersion(Version.Parse((string)document["fontVersion"]));

        if ((mask & FontMetadataMask.EngravingDefaults) != 0)
        {
            foreach (var property in TryGetChildrenProperties(document["engravingDefaults"]))
                sink.AddEngravingDefault(property.Name, DesignDecimal.Parse((string)property.Value));
        }

        if ((mask & FontMetadataMask.GlyphAnchors) != 0)
        {
            foreach (var glyphProperty in TryGetChildrenProperties(document["glyphsWithAnchors"]))
            {
                var key = GlyphKey.Parse(glyphProperty.Name);
                foreach (var anchorProperty in glyphProperty.Value.Children<JProperty>())
                    sink.AddGlyphAnchor(key, anchorProperty.Name, ParsePoint((JArray)anchorProperty.Value));
            }
        }

        if ((mask & FontMetadataMask.GlyphBoundingBoxes) != 0)
        {
            foreach (var glyphProperty in TryGetChildrenProperties(document["glyphBBoxes"]))
            {
                var key = GlyphKey.Parse(glyphProperty.Name);
                var southWest = ParsePoint((JArray)glyphProperty.Value["bBoxSW"]);
                var northEast = ParsePoint((JArray)glyphProperty.Value["bBoxNE"]);
                sink.AddGlyphBoundingBox(key, new DesignBox(southWest, northEast));
            }
        }

        // TODO: Parse more of the SMuFL font metadata
    }

    private static IEnumerable<JProperty> TryGetChildrenProperties(JToken token)
        => token is JObject obj ? obj.Properties() : Array.Empty<JProperty>();

    public static FontMetadata LoadFontMetadata(Stream stream, GlyphNameResolver glyphNameResolver,
        FontMetadataMask mask = FontMetadataMask.All)
    {
        if (!stream.CanRead) throw new ArgumentException();

        return LoadFontMetadata(new StreamReader(stream), glyphNameResolver, mask);
    }

    public static FontMetadata LoadFontMetadata(TextReader textReader, GlyphNameResolver glyphNameResolver,
        FontMetadataMask mask = FontMetadataMask.All)
    {
        var fontMetadata = new FontMetadata();
        var sink = fontMetadata.CreateSink(glyphNameResolver);
        FeedFontMetadata(textReader, sink, mask);
        return fontMetadata;
    }

    private static DesignPoint ParsePoint(JArray array)
        => new(DesignDecimal.Parse((string)array[0]), DesignDecimal.Parse((string)array[1]));
    #endregion

    #region Glyph Names
    public static IEnumerable<KeyValuePair<string, GlyphNameEntry>> EnumerateGlyphNames(Stream stream)
    {
        if (!stream.CanRead) throw new ArgumentException();

        return EnumerateGlyphNames(new StreamReader(stream));
    }

    public static IEnumerable<KeyValuePair<string, GlyphNameEntry>> EnumerateGlyphNames(TextReader textReader)
    {
        var root = JObject.Load(new JsonTextReader(textReader));
        foreach (var entry in root.Children<JProperty>())
        {
            var valueObject = (JObject)entry.Value;
            var codePoint = GlyphKey.TryParseUPlusNotation((string)valueObject["codepoint"])
                ?? throw new FormatException();
            var description = (string?)entry.Value["description"];
            yield return new KeyValuePair<string, GlyphNameEntry>(entry.Name, new GlyphNameEntry(codePoint, description));
        }
    }
    #endregion

    #region Classes
    public static IEnumerable<KeyValuePair<string, string>> EnumerateClasses(Stream stream)
    {
        if (!stream.CanRead) throw new ArgumentException();

        return EnumerateClasses(new StreamReader(stream));
    }

    public static IEnumerable<KeyValuePair<string, string>> EnumerateClasses(TextReader textReader)
    {
        var root = JObject.Load(new JsonTextReader(textReader));
        foreach (var classEntry in root.Children<JProperty>())
        {
            var className = classEntry.Name;
            foreach (var glyphName in classEntry.Value)
                yield return new KeyValuePair<string, string>(className, (string)glyphName);
        }
    }
    #endregion
}
