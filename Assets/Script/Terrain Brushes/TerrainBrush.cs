using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TerrainBrush
{
    /* STATIC */

    protected static void GetCursorBounds (out int xMin, out int xMax, out int yMin, out int yMax)
    {
        xMin = Mathf.Max (0, Mathf.FloorToInt (CameraController.Cursor.x - UserEditor.CursorSize));
        xMax = Mathf.Min (TerrainController.Width + 1, Mathf.CeilToInt (CameraController.Cursor.x + UserEditor.CursorSize));
        yMin = Mathf.Max (0, Mathf.FloorToInt (CameraController.Cursor.z - UserEditor.CursorSize));
        yMax = Mathf.Min (TerrainController.Height + 1, Mathf.CeilToInt (CameraController.Cursor.z + UserEditor.CursorSize));
    }



    /* INSTANCE */

    public abstract string Name { get; }
    public abstract string Tooltip { get; }
    public abstract int Order { get; }

    public abstract void Draw (float value);
}
