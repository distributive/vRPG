using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using LinkedStateList;

[RequireComponent (typeof (MeshFilter))]
public class TerrainController : MonoBehaviour
{
    /* STATIC */

    public static TerrainController Instance { private set; get; }

    // Size
    public static int Width { private set; get; }
    public static int Height { private set; get; }

    // Material
    public static Material Material { private set; get; }

    // Vertical limits
    public static float MaxElevation { private set; get; }
    public static float MinElevation { private set; get; }

    // Mesh
    public static Mesh Mesh { private set; get; }
    private static Vector3[] Vertices { set; get; }
    private static int[] Triangles { set; get; }



    // Assumes x and y are valid (x,y >= 0, x <= width, y <= height)
    public static float GetElevation (int x, int y)
    {
        return Vertices[x + y * (Width + 1)].y;
    }
    public static void SetElevation (int x, int y, float elevation)
    {
        int index = x + y * (Width + 1);
        Vertices[index] = new Vector3 (Vertices[index].x, Mathf.Clamp (elevation, MinElevation, MaxElevation), Vertices[index].z);
        
        HasChanged = true;
    }
    public static void OffsetElevation (int x, int y, float offset)
    {
        if (offset == 0)
            return;
        
        int index = x + y * (Width + 1);
        Vertices[index] = new Vector3 (Vertices[index].x, Mathf.Clamp (Vertices[index].y + offset, MinElevation, MaxElevation), Vertices[index].z);
        
        HasChanged = true;
    }

    // Returns a default for invalid inputs
    public static float GetElevationWithDefault (int x, int y, float defaultElevation)
    {
        int index = x + y * (Width + 1);
        return (index >= 0 && index < Vertices.Length) ? Vertices[index].y : defaultElevation;
    }



    // Reset mesh data to flat square
    public static void GenerateDefault ()
    {
        // Create grid of vertices
        Vertices = new Vector3[(Width + 1) * (Height + 1)];

        int i = 0;
        for (int y = 0; y <= Height; y++)
        {
            for (int x = 0; x <= Width; x++)
            {
                Vertices[i] = new Vector3 (x, 0, y);
                i++;
            }
        }

        // Create triangles
        Triangles = new int[Width * Height * 6];
        int vert = 0;
        int tris = 0;
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                Triangles[tris + 0] = vert;
                Triangles[tris + 1] = vert + Width + 1;
                Triangles[tris + 2] = vert + 1;
                Triangles[tris + 3] = vert + 1;
                Triangles[tris + 4] = vert + Width + 1;
                Triangles[tris + 5] = vert + Width + 2;

                vert++;
                tris += 6;
            }

            vert++;
        }

        // Save to edit history
        SaveState ();
    }



    // Regenerate mesh with current mesh data
    public static void UpdateMesh ()
    {
        Mesh.Clear ();

        Mesh.vertices = Vertices;
        Mesh.triangles = Triangles;

        Mesh.RecalculateNormals ();
    }



    // History
    private static LinkedStateList<TerrainData> editHistory;
    public static bool ActionOccuring { private set; get; } // Is set to true while a saveable action is currently being performed
    public static bool HasChanged { private set; get; } // Is set to false whenever the terrain is unchanged from the current saved state

    public static void SaveState ()
    {
        editHistory.Add (new TerrainData (Width, Height, Vertices, Triangles));

        HasChanged = false;
    }
    public static void Undo ()
    {
        if (!editHistory.CanGoToPrev)
            return;

        LoadTerrainData (editHistory.EnterPrevState ());
        UpdateMesh ();

        HasChanged = false;
    }
    public static void Redo ()
    {
        if (!editHistory.CanGoToNext)
            return;

        LoadTerrainData (editHistory.EnterNextState ());
        UpdateMesh ();

        HasChanged = false;
    }

    private static void LoadTerrainData (TerrainData data)
    {
        Width = data.width;
        Height = data.height;

        Vertices = new Vector3[data.vertices.Length];
        Triangles = new int[data.triangles.Length];

        //System.Array.Copy (data.vertices, Vertices, Vertices.Length);
        int i = 0;
        for (int y = 0; y <= Height; y++)
        {
            for (int x = 0; x <= Width; x++)
            {
                Vertices[i] = new Vector3 (x, data.vertices[i], y);
                i++;
            }
        }
        System.Array.Copy (data.triangles, Triangles, Triangles.Length);
    }



    // Action atomisation
    public static void BeginAction ()
    {
        ActionOccuring = true;
    }
    public static void EndAction ()
    {
        if (HasChanged)
            SaveState ();

        ActionOccuring = false;
        HasChanged = false;
    }



    // I/O
    public static void SaveToFile (string fileName)
    {
        // Format fileName correctly
        if (fileName[0] != '/')
            fileName = "/" + fileName;

        // Open file
        string destination = Application.persistentDataPath + fileName;
        FileStream file = (File.Exists (destination)) ? File.OpenWrite (destination) : File.Create (destination);

        // Write data
        BinaryFormatter bf = new BinaryFormatter ();
        SaveData data = new SaveData (editHistory.ToArray (), editHistory.CurrentState.index);
        bf.Serialize (file, data);
        
        // Close file
        file.Close ();
    }

    public static void LoadFromFile (string fileName)
    {
        // Format fileName correctly
        if (fileName[0] != '/')
            fileName = "/" + fileName;

        // Open file
        string destination = Application.persistentDataPath + fileName;
        FileStream file;

        // Check file exists
        if (File.Exists (destination))
        {
            file = File.OpenRead (destination);
        }
        else
        {
            Debug.LogError ("File not found");
            return;
        }

        // Load data
        BinaryFormatter bf = new BinaryFormatter ();
        SaveData data = (SaveData) bf.Deserialize (file);

        // Interpret data
        editHistory = LinkedStateList<TerrainData>.FromArray (data.editHistory, data.stateIndex);
        LoadTerrainData (editHistory.CurrentState.Value);

        // Update world
        UpdateMesh ();

        // Close file
        file.Close ();
    }



    // Update terrain material with cursor parameters
    public static void UpdateCursorPosition (Vector2 cursorPosition, float cursorSize, float cursorWeight)
    {
        Material.SetVector ("_CursorPosition", cursorPosition);
        Material.SetFloat ("_CursorSize", cursorSize);
        Material.SetFloat ("_CursorWeight", cursorWeight);
    }



    /* INSTANCE */

    // Size
    [SerializeField] private int width = 20, height = 20;

    // Vertical limtis
    [SerializeField] private float maxElevation = 10, minElevation = -10;

    // Material
    [SerializeField] private Material material;



    private void Awake ()
    {
        // Check for duplicate instances
        if (Instance != null)
            Destroy (gameObject);
        else
            Instance = this;

        // Size
        Width = width;
        Height = height;

        // Vertical limits
        MaxElevation = maxElevation;
        MinElevation = minElevation;

        // Material
        Material = material;

        // Edit history
        editHistory = new LinkedStateList<TerrainData> ();
    }

    private void Start ()
    {
        // Mesh
        Mesh = new Mesh ();
        GetComponent<MeshFilter> ().mesh = Mesh;

        GenerateDefault ();
        UpdateMesh ();
    }



    // TerrainData
    [System.Serializable]
    private class TerrainData
    {
        public int width, height;
        public float[] vertices;
        //public Vector3[] vertices;
        public int[] triangles;

        public TerrainData (int width, int height, Vector3[] vertices, int[] triangles)
        {
            this.width = width;
            this.height = height;

            this.vertices = new float[vertices.Length];
            this.triangles = new int[triangles.Length];

            //System.Array.Copy (vertices, this.vertices, vertices.Length);
            for (int i = 0; i < vertices.Length; i++)
            {
                this.vertices[i] = vertices[i].y;
            }
            System.Array.Copy (triangles, this.triangles, triangles.Length);
        }
    }



    // Save data
    [System.Serializable]
    private class SaveData
    {
        public TerrainData[] editHistory;
        public int stateIndex;

        public SaveData (TerrainData[] editHistory, int stateIndex)
        {
            this.editHistory = editHistory;
            this.stateIndex = stateIndex;
        }
    }



    /* DEBUG */
    //private void OnDrawGizmos ()
    //{
    //    if (Vertices == null)
    //        return;

    //    foreach (Vector3 vertex in Vertices)
    //    {
    //        Gizmos.DrawSphere (vertex, 0.1f);
    //    }

    //    for (int i = 0; i < Triangles.Length;  i += 3)
    //    {
    //        Gizmos.DrawLine (Vertices[Triangles[i    ]], Vertices[Triangles[i + 1]]);
    //        Gizmos.DrawLine (Vertices[Triangles[i + 1]], Vertices[Triangles[i + 2]]);
    //        Gizmos.DrawLine (Vertices[Triangles[i + 2]], Vertices[Triangles[i    ]]);
    //    }
    //}
}
