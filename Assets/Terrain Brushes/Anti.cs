using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Anti : TerrainBrush
{
    public override string Name { get { return "Anti"; } }
    public override string Tooltip { get { return "Anti"; } }
    public override int Order { get { return 500; } }

    public override void Draw (float value)
    {
        throw new System.NotImplementedException ();
    }
}
