using SebText.FontLoading;

namespace BoboEngine.UI;
public class TextRenderer : UIRenderer
{
    #region Instance Variables
    public FontData font { get => displayedFont; set => UpdateDisplayFont(value); }
    private FontData displayedFont;
    public string text { get => diplayedText; set => UpdateDisplayText(value); }

    private string diplayedText = "";
    #endregion

    #region Variable Update Functions
    private void UpdateDisplayFont(FontData font)
    {
        displayedFont = font;
        UpdateMesh();
    }
    private void UpdateDisplayText(string text)
    {
        diplayedText = text;
        UpdateMesh();
    }
    #endregion

    public override void Start()
    {
        base.Start();

        material.cullBackFaces = false;
        if (material.shaderID == "defaultUI") material.SetShaderID("defaultText");
    }

    private void UpdateMesh()
    {
        List<Float3> vertices = [];
        List<MeshVertex> vertexIndicies = [];
        List<MeshFace> faces = [];
        List<Float3> normalPositions = [];
        List<Float2> curveUVPositions = [new(0, 0), new(0.5f, 0), new(1f, 1f)];

        if (font != null && !string.IsNullOrEmpty(text))
        {
            int xOffset = 0;
            int vertI = 0;
            int charI = 0;

            foreach (var letter in text.ToCharArray())
            {
                font.TryGetGlyph(letter, out var glyph);

                xOffset += glyph.LeftSideBearing;

                if (letter == ' ')
                {
                    xOffset += glyph.AdvanceWidth;
                    continue;
                }

                var glyphs = ConnectContours(glyph.Glyphs);

                foreach (var points in glyphs.Select((item) => item.Reverse())) // Reverse order for counter clockwise triangle rendering
                {
                    int indexOffset = vertexIndicies.Count;

                    var curveClippedPoints = ClipOutsideCurves(points);

                    Engine.Log("Normal:");
                    _OutputDesmosPointArray(points, font.UnitsPerEm);
                    Engine.Log("On the curve:");
                    _OutputDesmosPointArray(curveClippedPoints, font.UnitsPerEm);

                    // On the curve mesh
                    foreach (var point in curveClippedPoints)
                    {
                        AddGlyphVert(TransformPoint(point.Position, xOffset, font.UnitsPerEm), glyph, charI++, vertI++, -1, ref vertices, ref normalPositions, ref vertexIndicies);
                    }

                    faces.AddRange(TriangulateEarClipping(vertices[indexOffset..], vertexIndicies[indexOffset..].ToArray()));

                    LoopedArray<FontData.Point> loopedPointsArray = new(points);

                    // Off the curve mesh
                    foreach (var centerItem in loopedPointsArray)
                    {
                        if (centerItem.value.OnCurve) continue;
                            
                        indexOffset = vertexIndicies.Count;

                        var previousItem = centerItem.Previous;
                        var nextItem = centerItem.Next;

                        AddGlyphVert(TransformPoint(previousItem.value.Position, xOffset, font.UnitsPerEm), glyph, charI++, vertI++, 0, ref vertices, ref normalPositions, ref vertexIndicies);
                        AddGlyphVert(TransformPoint(centerItem.value.Position, xOffset, font.UnitsPerEm), glyph, charI++, vertI++, 1, ref vertices, ref normalPositions, ref vertexIndicies);
                        AddGlyphVert(TransformPoint(nextItem.value.Position, xOffset, font.UnitsPerEm), glyph, charI++, vertI++, 2, ref vertices, ref normalPositions, ref vertexIndicies);

                        faces.Add(new MeshFace(vertexIndicies[indexOffset..]));
                    }
                }

                xOffset += glyph.AdvanceWidth;
            }
        }


        Mesh newMesh = new();
        newMesh.LoadRawData(vertices.ToArray(), faces.ToArray(), normalPositions.ToArray(), curveUVPositions.ToArray());
        meshFilter.mesh = newMesh;
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

    private static Float2 TransformPoint(Float2 point, float xOffset, int unitsPerEm)
    {
        return (point + Float2.xAxis * xOffset) / unitsPerEm;
    }
    private static void AddGlyphVert(Float2 vert, FontData.GlyphData glyph, int charI, int vertI, int uvI, ref List<Float3> vertices, ref List<Float3> normalPositions, ref List<MeshVertex> vertexIndicies)
    {
        vertices.Add(vert);

        var normalizedPoint = NormalizedUVChoordinate(glyph.Min, glyph.Max, vert);

        normalPositions.Add(new(normalizedPoint.x, normalizedPoint.y, charI));
        vertexIndicies.Add(new(vertI, uvI, vertI));
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

    private static MeshFace[] TriangulateEarClipping(IEnumerable<Float3> points, IEnumerable<MeshVertex> verts)
    {
        if (!points.Any()) return [];

        var remainingPoints = new LoopedArray<Float3>(points);

        List<MeshFace> results = [];

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
                    results.Add(new([verts.ElementAt(previousPoint.initalIndex), verts.ElementAt(currentPoint.initalIndex), verts.ElementAt(nextPoint.initalIndex)]));
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
                Engine.LogError("Unable to fully mesh out text!");
                break;
            }

            previousPoint = currentPoint;
            currentPoint = nextPoint;
            nextPoint = nextPoint.Next;
        }

        return results.ToArray();
    }


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
        return Float2.Dot((v0 - v1).Cross, v2 - v1) > 0.0001;
    }

    #endregion

    private static void _OutputDesmosPointArray(IEnumerable<FontData.Point> points, float scaler)
    {
        string deb = "";
        foreach (var point in points)
        {
            deb += $"{point.Position.x / scaler},{point.Position.y / scaler}\n";
        }
        Engine.LogMessage(deb);
    }

    public class NoValidBridgeException(string message) : Exception(message) { }
}
