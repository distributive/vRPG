using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterController : MonoBehaviour
{
    /* STATIC */

    public static WaterController Instance { private set; get; }

    // Size
    public static int Width { private set; get; }
    public static int Height { private set; get; }

    // Mesh
    public static Mesh Mesh { private set; get; }
    private static Vector3[] Vertices { set; get; }
    private static int[] Triangles { set; get; }



    // Generate water mesh data
    public static void GenerateMesh ()
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
    }

    // Generate mesh with current mesh data
    public static void UpdateMesh ()
    {
        Mesh.Clear ();

        Mesh.vertices = Vertices;
        Mesh.triangles = Triangles;

        Mesh.RecalculateNormals ();
    }



    /* INSTANCE */

    // Size
    [SerializeField] private int width = 20, height = 20;



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
    }

    private void Start ()
    {
        // Mesh
        Mesh = new Mesh ();
        GetComponent<MeshFilter> ().mesh = Mesh;

        GenerateMesh ();
        UpdateMesh ();
    }
}
