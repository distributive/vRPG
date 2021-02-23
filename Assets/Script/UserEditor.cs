using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserEditor : MonoBehaviour
{
    /* STATIC */

    // User
    public static float CursorSize { private set; get; }
    public static float CursorWeight { private set; get; }

    // Terrain brushes
    public static TerrainBrush TerrainBrush { get; set; }



    // Update brush values
    public static void SetSize (float value)
    {
        CursorSize = value;
    }
    public static void SetWeight (float value)
    {
        CursorWeight = value;
    }
    public static void ChangeSize (float value)
    {
        CursorSize += value;
    }
    public static void ChangeWeight (float value)
    {
        CursorWeight += value;
    }



    /* INSTANCE */

    // Cursor parameters
    [SerializeField] private float cursorRange = 5, cursorStrength = 1f;



    private void Awake ()
    {
        // User
        CursorSize = cursorRange;
        CursorWeight = cursorStrength;
    }



    private void Update ()
    {
        // Input
        if (TerrainBrush != null && Input.GetMouseButton (0) && !CameraController.IsPointerOverUIObject ())
        {
            if (Input.GetMouseButtonDown (0))
            {
                TerrainController.BeginAction ();
            }

            TerrainBrush.Draw (1);

            TerrainController.UpdateMesh ();
        }
        else if (TerrainController.ActionOccuring)
        {
            TerrainController.EndAction ();
        }

        // Hotkeys
        if (Input.GetKeyDown (KeyCode.K))
        {
            TerrainController.SaveToFile ("test");
        }
        else if (Input.GetKeyDown (KeyCode.L))
        {
            TerrainController.LoadFromFile ("test");
        }

        // Update cursor display on terrain
        Vector2 CursorPosition = new Vector2 (CameraController.Cursor.x, CameraController.Cursor.z);
        TerrainController.UpdateCursorPosition (CursorPosition, CursorSize, CursorWeight);
        WaterController.UpdateCursorPosition (CursorPosition, CursorSize, CursorWeight);
    }
}
