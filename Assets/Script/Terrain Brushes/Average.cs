using UnityEngine;
using Util;

public class Average : TerrainBrush
{
    public override string Name { get { return "Average"; } }
    public override string Tooltip { get { return "Average"; } }
    public override int Order { get { return 400; } }

    public override void Draw (float _)
    {
        GetCursorBounds (out int xMin, out int xMax, out int yMin, out int yMax);

        // Get average
        float average = 0;
        int count = 0;
        for (int x = xMin; x < xMax; x++)
        {
            for (int y = yMin; y < yMax; y++)
            {
                if (Vector3.Distance (new Vector3 (x, 0, y), CameraController.Cursor) <= UserEditor.CursorSize)
                {
                    average += TerrainController.GetElevation (x, y);
                    count++;
                }
            }
        }

        average /= count;

        // Tend each vertex to the average elevation
        for (int x = xMin; x < xMax; x++)
        {
            for (int y = yMin; y < yMax; y++)
            {
                float distance = Vector3.Distance (new Vector3 (x, 0, y), CameraController.Cursor);
                if (distance <= UserEditor.CursorSize)
                {
                    float normalisedDistance = 1 - distance / UserEditor.CursorSize;
                    float t = 10 * Time.deltaTime * normalisedDistance * MathfExt.Remap (UserEditor.CursorWeight, 0, 50, 0, 1);
                    float newElevation = Mathf.Lerp (TerrainController.GetElevation (x, y), average, t);

                    TerrainController.SetElevation (x, y, newElevation);
                }
            }
        }
    }
}
