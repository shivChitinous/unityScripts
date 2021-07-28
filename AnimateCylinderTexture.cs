using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimateCylinderTexture : MonoBehaviour
{
    private Material cylinderMaterial;

    void Start()
    {
        cylinderMaterial = Resources.Load(Janelia.CylinderBackgroundResources.MaterialName, typeof(Material)) as Material;
        if (cylinderMaterial == null)
        {
            Debug.LogError("Could not load material'" + Janelia.CylinderBackgroundResources.MaterialName + "'");
        }
    }

    void Update()
    {
        if (cylinderMaterial)
        {
            float x = Mathf.Sin(Time.time / 2) * 0.1f;
            float y = Mathf.Sin(Time.time) * 0.25f;
            Vector2 offset = new Vector2(x, y);
            cylinderMaterial.SetTextureOffset("_MainTex", offset);
        }
    }
}
