using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lower : TerrainBrush
{
    public override string Name { get { return "Lower"; } }
    public override string Tooltip { get { return "Lower"; } }
    public override int Order { get { return 200; } }

    public override void Draw (float _)
    {
        GetCursorBounds (out int xMin, out int xMax, out int yMin, out int yMax);

        for (int x = xMin; x < xMax; x++)
        {
            for (int y = yMin; y < yMax; y++)
            {
                float d = Vector3.Distance (new Vector3 (x, 0, y), CameraController.Cursor) / UserEditor.CursorSize;

                if (d <= 1)
                {
                    float offset = -UserEditor.CursorWeight * Time.deltaTime * ((1 - d) * (d + 1) * (1 - d) + d * (d - 1) * (d - 1));
                    TerrainController.OffsetElevation (x, y, offset);
                }
            }
        }
    }
}
