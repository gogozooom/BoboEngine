using SebText.FontLoading;
namespace BoboEngine.UI;

public class TextRenderer : UIRenderer
{
    public FontData font { get => displayedFont; set => UpdateDisplayFont(value); }
    private FontData displayedFont;
    private void UpdateDisplayFont(FontData font)
    {
        displayedFont = font;
        UpdateMesh();
    }
    public string text { get => diplayedText; set => UpdateDisplayText(value); }

    private string diplayedText = "";

    private void UpdateDisplayText(string text)
    {
        diplayedText = text;
        UpdateMesh();
    }

    private void UpdateMesh()
    {
        List<Float3> vertices = [];
        List<MeshVertex> vertexIndicies = [];
        List<MeshFace> faces = [];

        if (font != null && !string.IsNullOrEmpty(text))
        {
            int xOffset = 0;
            int vertI = 0;
            int startIndex;

            foreach (char letter in text.ToCharArray())
            {
                font.TryGetGlyph(letter, out var glyph);
                if (letter == ' ')
                {
                    xOffset += glyph.AdvanceWidth;
                    continue;
                }

                xOffset += glyph.LeftSideBearing;

                var points = LoopConnectShapes(SplitByIndices(glyph.Points, glyph.ContourEndIndices)).Reverse(); // Reverse order for counter clock wise

                startIndex = vertexIndicies.Count;

                _DebugLogPoints(points, font.UnitsPerEm);

                foreach (FontData.Point point in points) 
                {
                    vertices.Add(new((point.X + xOffset) / (float)font.UnitsPerEm, point.Y / (float)font.UnitsPerEm));
                    vertexIndicies.Add(new(vertI++, -1, -1));
                }

                faces.AddRange(TriangulateEarClipping(vertices[startIndex..], vertexIndicies[startIndex..].ToArray()));

                xOffset += glyph.AdvanceWidth;
            }
        }


        Mesh newMesh = new();
        newMesh.LoadRawData(vertices.ToArray(), faces.ToArray());
        meshFilter.mesh = newMesh;
    }

    private static void _DebugLogPoints(IEnumerable<FontData.Point> points, float scaler)
    {
        string deb = "";
        foreach (var point in points)
        {
            deb += $"{point.X / scaler},{point.Y / scaler}\n";
        }
        Engine.LogMessage(deb);
    }

    private static T[][] SplitByIndices<T>(T[] array, int[] splitIndices)
    {
        List<T[]> result = [];

        int startSplitIndex = 0;
        foreach (var i in splitIndices)
        {
            int endSplitIndex = i + 1;
                
            result.Add(array[startSplitIndex..endSplitIndex]);
            startSplitIndex = endSplitIndex;
        }

        return result.ToArray();
    }

    private static FontData.Point[] LoopConnectShapes(FontData.Point[][] pointCollections)
    {
        int[] startConnectIndices = [];
        int[] endConnectIndices = [];

        var builtShape = pointCollections[0];

        foreach (var insidePoints in pointCollections[1..])
        {
            (int outsideIndex, int insideIndex) = CalculateShapeCleanConnetionIndices(builtShape, insidePoints);

            builtShape = ConnectByIndices([builtShape, insidePoints], [outsideIndex], [insideIndex], true);
        }

        return builtShape;
    }

    public static T[] ConnectByIndices<T>(T[][] arrays, int[] startConnectIndices = null, int[] connectIndices = null, bool loopConnect = true, bool loopFirst = false)
    {
        List<T> result = [];

        if(arrays.Length == 0) return [];

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
                if(loopConnect) result.Add(item.value);
            }

            item = item.Next;
        }

        if (loopConnect && loopFirst) result.Add(array.GetItem(indexToStartWith).value);

        return result.ToArray();
    }


    /// <summary>
    /// Returns connection indices of two point arrays that cleanly connects a line without intersecting either shape
    /// </summary>
    /// <param name="insidePoints">The inside point shape array</param>
    /// <param name="outsidePoints">the outside point shape array</param>
    /// <returns></returns>
    private static (int outsideIndex, int insideIndex) CalculateShapeCleanConnetionIndices(FontData.Point[] outsidePoints, FontData.Point[] insidePoints)
    {
        LoopedArray<FontData.Point> insidePointsArray = new(insidePoints);
        LoopedArray<FontData.Point> outsidePointsArray = new(outsidePoints);

        var insideConnectionPoint = insidePointsArray.GetItem(0);
        var outsideConnectionPoint = outsidePointsArray.GetItem(0);

        // Loop through all possible inside connection points
        for (int x = 0; x < insidePoints.Length; x++)
        {
            // Loop through all possible outside connection points
            for (int y = 0; y < outsidePointsArray.Size; y++)
            {
                var possibleOutsideIntersectionPoint = outsideConnectionPoint.Next;

                bool isCollistion = false;

                // Check for line intersections with outside shape
                for (int z = 0; z < outsidePointsArray.Size - 1; z++)
                {
                    var nextPossibleOutsideIntersectionPoint = possibleOutsideIntersectionPoint.Next;
                    if (possibleOutsideIntersectionPoint == outsideConnectionPoint || nextPossibleOutsideIntersectionPoint == outsideConnectionPoint) continue;

                    if (DoLinesIntersect(insideConnectionPoint.value, outsideConnectionPoint.value, possibleOutsideIntersectionPoint.value, nextPossibleOutsideIntersectionPoint.value))
                    {
                        isCollistion = true;
                        break;
                    }

                    possibleOutsideIntersectionPoint = nextPossibleOutsideIntersectionPoint;
                }

                // Check for line intersections with inside shape
                if (!isCollistion)
                {
                    var possibleInsideIntersectionPoint = insideConnectionPoint.Next;

                    for (int z = 0; z < insidePointsArray.Size - 1; z++)
                    {
                        var nextPossibleInsideIntersectionPoint = possibleInsideIntersectionPoint.Next;

                        if (possibleInsideIntersectionPoint == insideConnectionPoint || nextPossibleInsideIntersectionPoint == insideConnectionPoint) continue;

                        if (DoLinesIntersect(insideConnectionPoint.value, outsideConnectionPoint.value, possibleInsideIntersectionPoint.value, nextPossibleInsideIntersectionPoint.value))
                        {
                            isCollistion = true;
                            break;
                        }

                        possibleInsideIntersectionPoint = nextPossibleInsideIntersectionPoint;
                    }
                }

                if (!isCollistion)
                    return (insideConnectionPoint.index, outsideConnectionPoint.index);

                outsideConnectionPoint = outsideConnectionPoint.Next;
            }

            insideConnectionPoint = insideConnectionPoint.Next;
        }

        // Nothing found
        return (-1, -1);
    }

    private MeshFace[] TriangulateEarClipping(IEnumerable<Float3> points, IEnumerable<MeshVertex> verts)
    {
        var remainingPoints = new LoopedArray<Float3>(points);

        List<MeshFace> results = [];

        string debugCreatedTriangles = "";

        if (remainingPoints.Size >= 3)
        {
            var currentPoint = remainingPoints.GetItem(0);
            var previousPoint = currentPoint.Previous;
            var nextPoint = currentPoint.Next;

            int tries = 0;

            while (remainingPoints.Size >= 3)
            {
                var currentVert = (Float2)currentPoint.value;
                var previousVert = (Float2)previousPoint.value;
                var nextVert = (Float2)nextPoint.value;

                var dotAngle = Float2.Dot((previousVert - currentVert).Cross, nextVert - currentVert);

                if (dotAngle > 0.0001)
                {
                    if (!IsAPointWithinTriangle(previousVert, currentVert, nextVert, points))
                    {
                        // Create Ear!
                        results.Add(new([verts.ElementAt(previousPoint.initalIndex), verts.ElementAt(currentPoint.initalIndex), verts.ElementAt(nextPoint.initalIndex)]));
                        // Remove current point
                        remainingPoints.RemoveItem(currentPoint);

                        debugCreatedTriangles += "P_{" + results.Count + "}=\\operatorname{polygon}\\left(\\left(x_{1}\\left[" + (previousPoint.initalIndex + 1) + "\\right],y_{1}\\left[" + (previousPoint.initalIndex + 1) + "\\right]\\right)," +
                            "\\left(x_{1}\\left[" + (currentPoint.initalIndex + 1) + "\\right],y_{1}\\left[" + (currentPoint.initalIndex + 1) + "\\right]\\right)," +
                            "\\left(x_{1}\\left[" + (nextPoint.initalIndex + 1) + "\\right],y_{1}\\left[" + (nextPoint.initalIndex + 1) + "\\right]\\right)\\right)\n";

                        // Shift points
                        currentPoint = nextPoint;
                        nextPoint = nextPoint.Next;
                        tries = 0;
                        continue;
                    }
                }


                debugCreatedTriangles += "F_{" + results.Count + "}=\\operatorname{polygon}\\left(\\left(x_{1}\\left[" + (previousPoint.initalIndex + 1) + "\\right],y_{1}\\left[" + (previousPoint.initalIndex + 1) + "\\right]\\right)," +
                    "\\left(x_{1}\\left[" + (currentPoint.initalIndex + 1) + "\\right],y_{1}\\left[" + (currentPoint.initalIndex + 1) + "\\right]\\right)," +
                    "\\left(x_{1}\\left[" + (nextPoint.initalIndex + 1) + "\\right],y_{1}\\left[" + (nextPoint.initalIndex + 1) + "\\right]\\right)\\right)\n";

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
        }

        //Engine.LogMessage("Debug Results: \n" + debugCreatedTriangles);

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

    public static bool IsAPointWithinTriangle(Float2 v0, Float2 v1, Float2 v2, IEnumerable<Float3> p)
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
}
