// Credit to Sebastian Lague for exploring fonts
// https://www.youtube.com/@SebastianLague

using BoboEngine;

namespace SebText.FontLoading;

// Contains the raw data loaded from a TrueType font file.
public class FontData
{
    public GlyphData[] Glyphs { get; private set; }
    public GlyphData MissingGlyph;
    public int UnitsPerEm;

    Dictionary<uint, GlyphData> glyphLookup;

    public FontData(GlyphData[] glyphs, int unitsPerEm)
    {
        Glyphs = glyphs;
        UnitsPerEm = unitsPerEm;
        glyphLookup = new();

        foreach (GlyphData c in glyphs)
        {
            if (c == null) continue;
            glyphLookup.Add(c.UnicodeValue, c);
            if (c.GlyphIndex == 0) MissingGlyph = c;
        }

        if (MissingGlyph == null) throw new Exception("No missing character glyph provided!");
    }

    public bool TryGetGlyph(uint unicode, out GlyphData character)
    {
        bool found = glyphLookup.TryGetValue(unicode, out character);
        if (!found)
        {
            character = MissingGlyph;
        }
        return found;
    }


    public class GlyphData
    {
        public uint UnicodeValue;
        public uint GlyphIndex;
        public GlyphPoints[] Glyphs;
        public Point[] Points;
        public int[] ContourEndIndices;
        public int AdvanceWidth;
        public int LeftSideBearing;

        public Int2 Min;
        public Int2 Max;

        public int Width => Max.x - Min.x;
        public int Height => Max.x - Min.x;
    }

    public class GlyphPoints(Point[] points, Int2 min, Int2 max, bool isContour)
    {
        public Point[] Points = points;
        public bool IsContour = isContour;

        public Int2 Min = min;
        public Int2 Max = max;
    }

    public struct Point(Int2 position, bool onCurve, bool isContour)
    {
        public Int2 Position = position;
        public bool OnCurve = onCurve;
        public bool IsContour = isContour;
    }
}