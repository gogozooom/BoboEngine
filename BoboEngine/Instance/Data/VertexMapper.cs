using System.Collections;
using Xunit.Internal;
using static BoboEngine.VertexMapper;

namespace BoboEngine;

public readonly struct VertexMapper : IEnumerable<VertexMap>
{
    public readonly List<FaceMap> faceMaps;
    public readonly int[][][] Data => faceMaps.SelectArray((faceMap) => faceMap.Data);

    public VertexMapper(IEnumerable<IEnumerable<IEnumerable<int>>> data)
    {
        faceMaps = data.SelectArray((faceData) => new FaceMap(faceData)).ToList();
    }
    public VertexMapper(IEnumerable<FaceMap> faceMaps) => this.faceMaps = faceMaps.ToList();
    public VertexMapper() => faceMaps = [];
    public readonly void Add(FaceMap v) => faceMaps.Add(v);
    public readonly void AddRange(IEnumerable<FaceMap> list)
    {
        foreach (var v in list)
        {
            faceMaps.Add(v);
        }
    }
    public readonly void Add(string objFaceElements)
    {
        string[] elements = objFaceElements.Split(' ')[1..];

        FaceMap faceMap = new();

        foreach (var vertexInfo in elements)
        {
            var indexes = vertexInfo.Split('/');

            int vertex_coord_index = -1;
            int texture_coord_index = -1;
            int normal_coord_index = -1;

            if (indexes.Length > 0 && int.TryParse(indexes[0], out var index1))
            {
                vertex_coord_index = index1 - 1; // converted to 0 indexed list format
            }

            if (indexes.Length > 1 && int.TryParse(indexes[1], out var index2))
            {
                texture_coord_index = index2 - 1; // converted to 0 indexed list format
            }

            if (indexes.Length > 2 && int.TryParse(indexes[2], out var index3))
            {
                normal_coord_index = index3 - 1; // converted to 0 indexed list format
            }

            faceMap.Add(new([vertex_coord_index,texture_coord_index,normal_coord_index]));
        }

        Add(faceMap);
    }

    public readonly byte[] glConvertToData(VertexData data)
    {
        List<byte> result = [];

        foreach (var vertexMap in this)
        {
            for (int i = 0; i < data.attributes.Count; i++)
            {
                var paramIndex = i < vertexMap.indexMaps.Count ? vertexMap.indexMaps[i] : -1; // Missing data gets set to -1
                var attribute = data.attributes[i];
                var glConvertItemToData = attribute.GetType().GetMethod("glConvertItemToData");
                object[] param = [paramIndex];

                IEnumerable<byte> dataResult = (IEnumerable<byte>)glConvertItemToData.Invoke(attribute, param);

                result.AddRange(dataResult);
            }
        }

        return result.ToArray();
    }

    public readonly VertexMapper Triangulated()
    {
        List<FaceMap> result = [];

        foreach (var faceMap in faceMaps)
        {
            if (faceMap.vertexMaps.Count <= 3)
            {
                result.Add(faceMap);
                continue;
            }

            for (int i = 2; i < faceMap.vertexMaps.Count; i++)
            {
                result.Add(new FaceMap((VertexMap[])[faceMap.vertexMaps[0], faceMap.vertexMaps[i - 1], faceMap.vertexMaps[i]]));
            }
        }

        return new(result);
    }

    public readonly bool IsEmpty()
    {
        return faceMaps == null || faceMaps.Count == 0;
    }

    public IEnumerator<VertexMap> GetEnumerator()
    {
        foreach (var face in faceMaps)
        {
            foreach(var vertex in face)
            {
                yield return vertex;
            } 
        }
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public readonly struct FaceMap : IEnumerable<VertexMap>
    {
        public readonly List<VertexMap> vertexMaps;
        public readonly int[][] Data => vertexMaps.SelectArray((vertexMap) => vertexMap.Data);

        public FaceMap(IEnumerable<IEnumerable<int>> data)
        {
            vertexMaps = data.SelectArray((vertexData) => new VertexMap(vertexData)).ToList();
        }
        public FaceMap(IEnumerable<VertexMap> vertexMaps) => this.vertexMaps = vertexMaps.ToList();
        public FaceMap() => vertexMaps = [];
        public readonly void Add(VertexMap v) => vertexMaps.Add(v);

        public IEnumerator<VertexMap> GetEnumerator()
        {
            foreach (var vertex in vertexMaps)
            {
                yield return vertex;
            }
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public readonly struct VertexMap : IEnumerable<int>
    {
        public readonly List<int> indexMaps;
        public readonly int[] Data => indexMaps.ToArray();

        public VertexMap(IEnumerable<int> data) => indexMaps = data.ToList();
        public VertexMap() => indexMaps = [];

        public readonly void Add(int v) => indexMaps.Add(v);

        public IEnumerator<int> GetEnumerator()
        {
            foreach (var index in indexMaps)
            {
                yield return index;
            }
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
