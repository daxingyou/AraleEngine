using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIRaycast : MaskableGraphic
{
	protected UIRaycast()
    {
        useLegacyMeshGeneration = false;
    }

    protected override void OnPopulateMesh(VertexHelper toFill)
    {
        toFill.Clear();
    }
}
