using SebText.FontLoading;
using System.Text.RegularExpressions;

namespace BoboEngine.UI;
public class TextRenderer : UIRenderer
{
    #region Instance Variables
    public FontData font { get => _font; set => UpdateFont(value); }
    private FontData _font;
    public string text { get => _text; set => UpdateText(value); }
    private string _text = "";
    public float fontSize { get => _fontSize; set => UpdateFontSize(value); }
    private float _fontSize = 20;
    public bool textWrap { get => _textWrap; set => UpdateTextWrap(value); }
    private bool _textWrap = false;

    #endregion

    #region Variable Update Functions
    private void UpdateFont(FontData font)
    {
        _font = font;
        UpdateMesh();
    }
    private void UpdateText(string text)
    {
        _text = text;
        UpdateMesh();
    }
    private void UpdateFontSize(float v)
    {
        _fontSize = v;
        UpdateMesh();
    }
    private void UpdateTextWrap(bool v)
    {
        _textWrap = v;
        UpdateMesh();
    }
    private void OnWindowSizeChange() => UpdateMesh();
    private void OnTransformChange() => UpdateMesh();
    #endregion

    public override void Start()
    {
        base.Start();

        material.cullBackFaces = false;
        uiTransform.onTransformUpdate += OnTransformChange;
        WindowManager.onWindowSizeChange += OnWindowSizeChange;
        if (material.shaderID == "defaultUI") material.SetShaderID("defaultText");
    }

    private void UpdateMesh()
    {
        if (font == null || string.IsNullOrEmpty(text))
        {
            meshFilter.mesh = null;
            return;
        }

        VertexData data = new();
        VertexMapper mapper = new();

        CreateMesh(ref data, ref mapper);
        //DEBUG_CreateSquare(ref data, ref mapper);
        
        Mesh newMesh = new();
        newMesh.LoadRawData(data, mapper);
        meshFilter.mesh = newMesh;
    }

    public ((string text, int width)[] lines, int width, int height) CalculateBounds()
    {
        List<(string text, int width)> lines = [];
        int height = font.UnitsPerEm;

        int width = 0;
        List<int> xOffsets = [];
        int xOffset = 0;
        int words = 0;

        float windowWidth = uiTransform.pixelScale.x * 0.5f * font.UnitsPerEm / fontSize;

        string buffer = "";

        foreach (var letter in text.ToCharArray())
        {
            font.TryGetGlyph(letter, out var glyph);

            var advance = glyph.AdvanceWidth + glyph.LeftSideBearing;
            var lineFits = xOffset + advance <= windowWidth;

            if (letter == ' ' && Regex.IsMatch(buffer, @"^[\w]"))
            {
                words++;
            }
            else if (textWrap && !lineFits && buffer.Length > 0)
            {
                string carry = $"{letter}";
                if (words > 0)
                {
                    var split = buffer.Split(' ');
                    carry = split.Last()+letter;
                    buffer = string.Join(' ', split[..(split.Length - 1)]);
                    xOffsets = xOffsets[buffer.Length..];
                }
                else
                {
                    xOffsets = [];
                }

                if (xOffset > width) width = xOffset;
                lines.Add((buffer + "\n", xOffset));

                xOffset = xOffsets.Sum();
                words = 0;
                buffer = $"{carry}";

                height += font.UnitsPerEm;
                continue;
            }
            else if (letter == '\n')
            {
                if (xOffset > width) width = xOffset;
                lines.Add((buffer + "\n", xOffset));

                xOffset = 0;
                xOffsets = [];
                words = 0;
                buffer = "";

                height += font.UnitsPerEm;
                continue;
            }

            xOffsets.Add(advance);
            xOffset += advance;

            buffer += letter;
        }

        lines.Add((buffer, xOffset));

        if (xOffset > width) width = xOffset;

        return (lines.ToArray(), width, height);
    }
    private void CreateMesh(ref VertexData data, ref VertexMapper mapper)
    {
        data ??= new();

        var vertices = data.AddAttribute<Float3>("vertices", 0);
        var curveUVPositions = data.AddAttribute<Float2>("textureCoords", 1);
        var normalPositions = data.AddAttribute<Float3>("normals", 2);
        curveUVPositions.Populate([new(0, 0), new(0.5f, 0), new(1f, 1f)]);
        var isClockwise = data.AddAttribute<bool>("isClockwise", 3);
        isClockwise.Populate([true]);

        // TODO: Create a preprossessing step that separates a string into lines and conencted characters/words
        // If current line is bigger than ui scale, move current word buffer into next line
        // Then when creating the mesh, init x and y offsets to match based on which vertical and horizontal align methods set
        // MAYBE?: Create autoFontScale by running the preprossessing step at max size until it can't fit, scale down then try again
        var (lines, width, height) = CalculateBounds();

        int xOffset = 0;
        int yOffset = 0;
        int vertI = vertices.Count;
        int charI = 0;

        float unitsPerEm = 1f / font.UnitsPerEm;

        foreach (var line in lines)
        {
            foreach (var letter in line.text.ToCharArray())
            {
                font.TryGetGlyph(letter, out var glyph);

                if (letter == ' ')
                {
                    xOffset += glyph.AdvanceWidth;
                    continue;
                }

                if (letter == '\n')
                {
                    xOffset = 0;
                    yOffset -= font.UnitsPerEm;
                    continue;
                }

                xOffset += glyph.LeftSideBearing;

                var glyphs = ConnectContours(glyph.Glyphs);

                foreach (var points in glyphs.Select((item) => item.Reverse())) // Reverse order for counter clockwise triangle rendering
                {
                    int indexOffset = vertices.Count;

                    var curveClippedPoints = ClipOutsideCurves(points);

                    //Engine.Log("Normal:");
                    //DEBUG_OutputDesmosPointArray(points);
                    //Engine.Log("On the curve:");
                    //DEBUG_OutputDesmosPointArray(curveClippedPoints);

                    List<VertexMapper.VertexMap> glyphVertMaps = [];

                    // On the curve mesh
                    foreach (var point in curveClippedPoints)
                    {
                        AddGlyphVert(point.Position, new(xOffset, yOffset), unitsPerEm, glyph, charI, vertI++, -1, false, ref vertices, ref normalPositions, ref glyphVertMaps);
                    }

                    mapper.AddRange(TriangulateEarClipping(vertices.values[indexOffset..], glyphVertMaps));

                    LoopedArray<FontData.Point> loopedPointsArray = new(points);

                    // Off the curve mesh
                    foreach (var centerItem in loopedPointsArray)
                    {
                        if (centerItem.value.OnCurve) continue;

                        indexOffset = vertices.values.Count;

                        var previousItem = centerItem.Previous;
                        var nextItem = centerItem.Next;

                        bool clockwise = !IsTriangleCounterClockwise(previousItem.value.Position, centerItem.value.Position, nextItem.value.Position);

                        List<VertexMapper.VertexMap> vertMaps = [];

                        AddGlyphVert(centerItem.value.Position, new(xOffset, yOffset), unitsPerEm, glyph, charI, vertI++, 1, clockwise, ref vertices, ref normalPositions, ref vertMaps);
                        AddGlyphVert(nextItem.value.Position, new(xOffset, yOffset), unitsPerEm, glyph, charI, vertI++, 2, clockwise, ref vertices, ref normalPositions, ref vertMaps);
                        AddGlyphVert(previousItem.value.Position, new(xOffset, yOffset), unitsPerEm, glyph, charI, vertI++, 0, clockwise, ref vertices, ref normalPositions, ref vertMaps);

                        mapper.Add(new VertexMapper.FaceMap(vertMaps));
                    }
                }

                charI++;
                xOffset += glyph.AdvanceWidth;
            }
        }
    }

    protected override void glSetMatrixes()
    {
        material.shader.glSetMatrix4x4("model", transform.Matrix * UITransform.CreateMatrixScale(new Float2(fontSize, fontSize), new(0,0,0,0)) * uiTransform.MatrixTrans);
    }

    #region Helper Functions

    private static FontData.Point[] ClipOutsideCurves(IEnumerable<FontData.Point> points)
    {
        List<FontData.Point> result = [];

        foreach (var point in new LoopedArray<FontData.Point>(points))
        {
            var PreviousPoint = point.Previous;
            var NextPoint = point.Next;

            if (!point.value.OnCurve && IsTriangleCounterClockwise(PreviousPoint.value.Position, point.value.Position, NextPoint.value.Position)) continue;

            result.Add(point.value);
        }

        return result.ToArray();
    }

    private static void AddGlyphVert(Float2 glyphPos, Float2 offset, float UnitsPerEm, FontData.GlyphData glyph, int charI, int vertI, int uvI, bool isClockwise, ref VertexAttribute<Float3> vertices, ref VertexAttribute<Float3> normalPositions, ref List<VertexMapper.VertexMap> vertMaps)
    {
        var normalizedPoint = NormalizedUVChoordinate(glyph.Min, glyph.Max, glyphPos);
        vertices.Add(TransformPoint(glyphPos, offset, UnitsPerEm));
        normalPositions.Add(new(normalizedPoint.x, normalizedPoint.y, charI));

        vertMaps.Add([vertI, uvI, vertI, isClockwise ? 0 : -1]);
    }
    private static Float2 TransformPoint(Float2 point, Float2 offset, float unitsPerEm)
    {
        return (point + offset) * unitsPerEm;
    }

    public static T[] ConnectByIndices<T>(T[][] arrays, int[] startConnectIndices = null, int[] connectIndices = null, bool loopConnect = true, bool loopFirst = false)
    {
        List<T> result = [];

        if (arrays.Length == 0) return [];

        LoopedArray<T> array = new(arrays[0]);

        bool startConnectionIndicesExist = startConnectIndices != null && startConnectIndices.Length > 0;
        bool connectionIndicesExist = connectIndices != null && connectIndices.Length > 0;

        int indexToStartWith = startConnectionIndicesExist ? startConnectIndices[0] : 0;
        int indexToConnect = connectionIndicesExist ? connectIndices[0] : -1;

        var connectIndexOutOfRange = indexToConnect < 0 || indexToConnect >= array.Size;

        var item = array.GetItem(indexToStartWith);

        for (int i = 0; i < array.Size; i++)
        {
            var isLastItem = i == array.Size - 1;

            result.Add(item.value);

            if (indexToConnect == item.index || isLastItem && connectIndexOutOfRange && arrays.Length > 1)
            {
                var remainingArrays = arrays.Length > 0 ? arrays[1..] : [];
                var remainingStartConnectionIndices = startConnectionIndicesExist ? startConnectIndices[1..] : [];
                var remainingConnectionIndices = connectionIndicesExist ? connectIndices[1..] : [];

                result.AddRange(ConnectByIndices(remainingArrays, remainingStartConnectionIndices, remainingConnectionIndices, loopConnect, true));
                if (loopConnect) result.Add(item.value);
            }

            item = item.Next;
        }

        if (loopConnect && loopFirst) result.Add(array.GetItem(indexToStartWith).value);

        return result.ToArray();
    }

    private static FontData.Point[][] ConnectContours(FontData.GlyphPoints[] pointCollections)
    {
        List<FontData.Point[]> result = [];

        List<FontData.GlyphPoints> baseGlyphs = [];
        List<FontData.GlyphPoints> contourGlyphs = [];

        foreach (var glyph in pointCollections)
        {
            (glyph.IsContour ? contourGlyphs : baseGlyphs).Add(glyph);
        }

        foreach (var baseGlyph in baseGlyphs)
        {
            FontData.Point[] combinedGlyph = baseGlyph.Points;

            foreach (var contourGlyph in contourGlyphs.ToArray())
            {
                // Is contourGlyph within baseGlyph?
                if (contourGlyph.Max.x > baseGlyph.Max.x ||
                    contourGlyph.Max.y > baseGlyph.Max.y ||
                    contourGlyph.Min.x < baseGlyph.Min.x ||
                    contourGlyph.Min.y < baseGlyph.Min.y) continue;

                (int outsideIndex, int insideIndex) = CalculateShapeCleanConnetionIndices(combinedGlyph, contourGlyph.Points);

                if (outsideIndex == -1 || insideIndex == -1) throw new NoValidBridgeException("No valid contour bridge could be found for this character! This should be immpossible!");

                combinedGlyph = ConnectByIndices([combinedGlyph, contourGlyph.Points], [outsideIndex], [insideIndex], true);
                contourGlyphs.Remove(contourGlyph);
            }

            result.Add(combinedGlyph);
        }

        if(contourGlyphs.Count > 0)
        {
            Engine.LogWarning("Contour glyphs still remain when building shape! Must be invalid font!");
        }

        return result.ToArray();
    }

    /// <summary>
    /// Returns connection indices of two point arrays that cleanly connects a line without intersecting either shape
    /// </summary>
    /// <param name="basePoints">the outside point shape array</param>
    /// <param name="contourPoints">The inside point shape array</param>
    /// <returns></returns>
    private static (int outsideIndex, int insideIndex) CalculateShapeCleanConnetionIndices(FontData.Point[] basePoints, FontData.Point[] contourPoints)
    {
        LoopedArray<FontData.Point> contourPointsArray = new(contourPoints);
        LoopedArray<FontData.Point> basePointsArray = new(basePoints);


        foreach (var contourConnectionPoint in contourPointsArray)
        {
            foreach (var baseConnectionPoint in basePointsArray)
            {
                if (!baseConnectionPoint.value.OnCurve) continue;
                var possibleOutsideIntersectionPoint = baseConnectionPoint.Next;

                bool isCollistion = false;

                // Check for line intersections with outside shape
                for (int z = 0; z < basePointsArray.Size - 1; z++)
                {
                    var nextPossibleOutsideIntersectionPoint = possibleOutsideIntersectionPoint.Next;
                    if (possibleOutsideIntersectionPoint == baseConnectionPoint || nextPossibleOutsideIntersectionPoint == baseConnectionPoint) continue;

                    if (DoLinesIntersect(contourConnectionPoint.value.Position, baseConnectionPoint.value.Position, possibleOutsideIntersectionPoint.value.Position, nextPossibleOutsideIntersectionPoint.value.Position))
                    {
                        isCollistion = true;
                        break;
                    }

                    possibleOutsideIntersectionPoint = nextPossibleOutsideIntersectionPoint;
                }

                // Check for line intersections with inside shape
                if (!isCollistion)
                {
                    if (!contourConnectionPoint.value.OnCurve) continue;
                    var possibleInsideIntersectionPoint = contourConnectionPoint.Next;

                    for (int z = 0; z < contourPointsArray.Size - 1; z++)
                    {
                        var nextPossibleInsideIntersectionPoint = possibleInsideIntersectionPoint.Next;

                        if (possibleInsideIntersectionPoint == contourConnectionPoint || nextPossibleInsideIntersectionPoint == contourConnectionPoint) continue;

                        if (DoLinesIntersect(contourConnectionPoint.value.Position, baseConnectionPoint.value.Position, possibleInsideIntersectionPoint.value.Position, nextPossibleInsideIntersectionPoint.value.Position))
                        {
                            isCollistion = true;
                            break;
                        }

                        possibleInsideIntersectionPoint = nextPossibleInsideIntersectionPoint;
                    }
                }

                if (!isCollistion)
                    return (contourConnectionPoint.index, baseConnectionPoint.index);
            }
        }

        // Nothing found
        return (-1, -1);
    }

    private static List<VertexMapper.FaceMap> TriangulateEarClipping(IEnumerable<Float3> points, IEnumerable<VertexMapper.VertexMap> verts)
    {
        if (!points.Any()) return [];

        var remainingPoints = new LoopedArray<Float3>(points);

        List<VertexMapper.FaceMap> results = [];

        var currentPoint = remainingPoints.GetItem(0);
        var previousPoint = currentPoint.Previous;
        var nextPoint = currentPoint.Next;

        int tries = 0;

        while (remainingPoints.Size >= 3)
        {
            var currentVert = (Float2)currentPoint.value;
            var previousVert = (Float2)previousPoint.value;
            var nextVert = (Float2)nextPoint.value;

            if (IsTriangleCounterClockwise(previousVert, currentVert, nextVert))
            {
                if (!IsAVertexWithin2DTriangle(previousVert, currentVert, nextVert, points))
                {
                    // Create Ear!
                    results.Add([verts.ElementAt(previousPoint.initalIndex), verts.ElementAt(currentPoint.initalIndex), verts.ElementAt(nextPoint.initalIndex)]);
                    //DEBUG_OutputDesmosPolygon(previousPoint.initalIndex+1, currentPoint.initalIndex+1, nextPoint.initalIndex+1);

                    // Remove current point
                    remainingPoints.RemoveItem(currentPoint);

                    // Shift points
                    currentPoint = nextPoint;
                    nextPoint = nextPoint.Next;
                    tries = 0;
                    continue;
                }
            }

            tries++;

            if (tries >= remainingPoints.Size)
            {
                //Engine.LogError("Unable to fully mesh out text!");
                break;
            }

            previousPoint = currentPoint;
            currentPoint = nextPoint;
            nextPoint = nextPoint.Next;
        }

        return results;
    }

    #region math
    public static bool DoLinesIntersect(Float2 a1, Float2 a2, Float2 b1, Float2 b2, float margin = 0.0001f)
    {
        // Desmos: https://www.desmos.com/geometry/sc4kt0prnx

        var aDif = a2 - a1;
        var bDif = b2 - b1;
        var aLength = aDif.Length;
        var bLength = bDif.Length;

        if (aLength == 0 || bLength == 0) return false; // Points not allowed

        var rotator = BaseVectors2D.FromRotation(-aDif.Angle());

        Float2 t1 = rotator.TransformVector(b1 - a1);
        Float2 t2 = rotator.TransformVector(b2 - a1);

        var b1TouchingYIntercept = float.Abs(t1.x) < margin;
        var b2TouchingYIntercept = float.Abs(t2.x) < margin;
        var bCrossingYIntercept = float.Sign(t1.x) != float.Sign(t2.x);

        if (!b1TouchingYIntercept && !b2TouchingYIntercept && !bCrossingYIntercept) return false;

        var xIntercept = t1.y - t1.x * (t2 - t1).Slope;

        // t's xIntercept falls in correct range?
        return xIntercept >= -margin && xIntercept <= aLength + margin;
    }
    public static bool IsAVertexWithin2DTriangle(Float2 v0, Float2 v1, Float2 v2, IEnumerable<Float3> p)
    {
        foreach (var point3d in p)
        {
            var otherPoint = (Float2)point3d;
            if (v0 == otherPoint ||
                v1 == otherPoint ||
                v2 == otherPoint) continue;

            if (IsPointWithinTriangle(v0, v1, v2, otherPoint))
            {
                return true;
            }
        }

        return false;
    }
    public static bool IsPointWithinTriangle(Float2 v0, Float2 v1, Float2 v2, Float2 p)
    {
        // Desmos: https://www.desmos.com/calculator/wpz7qxubrx
        var v0CrossVector = (v2 - v0).Cross;
        var v1CrossVector = (v0 - v1).Cross;
        var v2CrossVector = (v1 - v2).Cross;

        bool v0CheckPasses = Float2.Dot(p - v0, v0CrossVector) >= 0;
        bool v1CheckPasses = Float2.Dot(p - v1, v1CrossVector) >= 0;
        bool v2CheckPasses = Float2.Dot(p - v2, v2CrossVector) >= 0;

        return v0CheckPasses && v1CheckPasses && v2CheckPasses;
    }
    public static Float2 NormalizedUVChoordinate(Float2 min, Float2 max, Float2 p)
    {
        return (p - min) / (max - min);
    }
    public static bool IsTriangleCounterClockwise(Float2 v0, Float2 v1, Float2 v2)
    {
        return Float2.Dot((v0 - v1).Cross, v2 - v1) > 0.00001;
    }
    public static (float v0Inf, float v1Inf, float v2Inf) GetPointsOfTriangleInfluence(Float2 v0, Float2 v1, Float2 v2, Float2 p, bool clamped = true)
    {
        var v0MeasureVector = (v2 - v1).Cross;
        var v1MeasureVector = (v0 - v2).Cross;
        var v2MeasureVector = (v1 - v0).Cross;

        var pV0MeasureDot = Float2.Dot(v0MeasureVector, p - v1);
        var pV1MeasureDot = Float2.Dot(v1MeasureVector, p - v2);
        var pV2MeasureDot = Float2.Dot(v2MeasureVector, p - v0);

        var v0ReferenceDot = Float2.Dot(v0MeasureVector, v0 - v1);
        var v1ReferenceDot = Float2.Dot(v1MeasureVector, v1 - v2);
        var v2ReferenceDot = Float2.Dot(v2MeasureVector, v2 - v0);

        var aInf = pV0MeasureDot / v0ReferenceDot;
        var bInf = pV1MeasureDot / v1ReferenceDot;
        var cInf = pV2MeasureDot / v2ReferenceDot;

        if (clamped)
        {
            aInf = Maths.Clamp(aInf, 0, 1);
            bInf = Maths.Clamp(bInf, 0, 1);
            cInf = Maths.Clamp(cInf, 0, 1);
        }

        return (aInf,bInf,cInf);
    }
    #endregion

    #endregion

    private static void DEBUG_OutputDesmosPointArray(IEnumerable<FontData.Point> points)
    {
        string deb = "";
        foreach (var point in points)
        {
            deb += $"{point.Position.x},{point.Position.y}\n";
        }
        Engine.LogMessage(deb);
    }
    private static void DEBUG_OutputDesmosPolygon(int lastPoint, int currentPoint, int nextPoint)
    {
        Console.WriteLine($"P=\\operatorname{{polygon}}\\left(\\left(x_{{2}}\\left[{lastPoint}\\right],y_{{2}}\\left[{lastPoint}\\right]\\right),\\left(x_{{2}}\\left[{currentPoint}\\right],y_{{2}}\\left[{currentPoint}\\right]\\right),\\left(x_{{2}}\\left[{nextPoint}\\right],y_{{2}}\\left[{nextPoint}\\right]\\right)\\right)");
    }
    private static void DEBUG_CreateSquare(ref VertexData data, ref VertexMapper mapper)
    {
        var vertices = data.AddAttribute<Float3>("vertices", 0);
        var normalPositions = data.AddAttribute<Float3>("normals", 2);

        int index = vertices.Count;

        vertices.AddRange([new(0, 0, 0.01f), new(0.5f, 0, 0.01f), new(0.5f, 0.5f, 0.01f), new(0, 0.5f, 0.01f)]);
        normalPositions.AddRange([new(0, 0, 0), new(1, 0, 0), new(1, 1, 0), new(0, 1, 0)]);
        int[][] d = [[index, -1, index++], [index, -1, index++], [index, -1, index++], [index, -1, index]];
        mapper.Add(new VertexMapper.FaceMap(d));
    }

    public class NoValidBridgeException(string message) : Exception(message) { }
}
