using BoboEngine.GMath;

namespace BoboEngine
{
    public class Mesh : ObjectBehavior
    {
        public float3[] vertices;
        public int3[] faces;
        public float3[] faceColors;

        public Mesh()
        {

        }

        public bool LoadObjFile(string filePath)
        {
            // -- Error Checks --
            if (string.IsNullOrEmpty(filePath))
            {
                Program.LogError($"Please specify a file to load!");
                return false;
            }
            if (!File.Exists(filePath))
            {
                Program.LogError($"Could find model file '{filePath}'");
                return false;
            }

            string[] fileNameL = filePath.Split('.');

            string fileName = fileNameL[fileNameL.Length - 1];

            if (fileName != "obj")
            {
                Program.LogError($"'{fileName}' file type unsupported! Please supply an .obj file!");
                return false;
            }

            List<float3> vertices = new();
            List<int3> faces = new();
            List<float3> faceColors = new();

            string[] data = File.ReadAllLines(filePath);

            foreach (var line in data)
            {
                if (line.StartsWith("v ")) // Vertex Info
                {
                    float[] axes = line[2..].Split(' ').Select(float.Parse).ToArray();
                    vertices.Add(new(-axes[0], axes[1], axes[2])); // Flip x to make positive x go right
                }
                else if (line.StartsWith("f ")) // Face Info
                {
                    string[] faceIndexGroups = line[2..].Split(' ');

                    int[] indexs = new int[faceIndexGroups.Length];

                    float3 faceColor = float3.random;

                    for (int i = 0; i < faceIndexGroups.Length; i++)
                    {
                        indexs[i] = int.Parse(faceIndexGroups[i].Split('/')[0]) - 1;

                        if (i == 2) // 3rd Vertex
                        {
                            faces.Add((int3)indexs);
                            faceColors.Add(float3.random);
                        }
                        else if (i > 2) // 3rd Onward Vertices TODO: possible issue with concave mesh
                        {
                            int[] continuedIndex = [indexs[0], indexs[i - 1], indexs[i]];

                            faces.Add((int3)continuedIndex);
                            faceColors.Add(float3.random);
                        }
                    }
                }
            }

            this.vertices = vertices.ToArray();
            this.faces = faces.ToArray();
            this.faceColors = faceColors.ToArray();

            return true;
        }

        public bool IsEmpty()
        {
            if (vertices == null) return true;
            else if (vertices.Length == 0) return true;

            if (faces == null) return true;
            else if (faces.Length == 0) return true;

            // Face colors not nessesary 

            return false;
        }

        public static string GetLocalModelPath(string model)
        {
            return Path.Combine(Program.ProgramDirectory, "Models", model + ".obj");
        }
    }
}