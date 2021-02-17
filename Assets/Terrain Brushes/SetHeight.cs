using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetHeight : TerrainBrush
{
    public override string Name { get { return "Set Height"; } }
    public override string Tooltip { get { return "Set height"; } }
    public override int Order { get { return 300; } }

    public override void Draw (float targetElevation)
    {
        GetCursorBounds (out int xMin, out int xMax, out int yMin, out int yMax);

        // Tend each vertex to the target elevation
        for (int x = xMin; x < xMax; x++)
        {
            for (int y = yMin; y < yMax; y++)
            {
                if (Vector3.Distance (new Vector3 (x, 0, y), CameraController.Cursor) <= TerrainController.CursorSize)
                {
                    TerrainController.SetElevation (x, y, targetElevation);
                }
            }
        }
    }
}
