using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR

public class Populate : MonoBehaviour
{
    [MenuItem("Window/Populate Planar")]
    public static void run()
    {
        
        
        string[] colors = { "#555555" }; // just black
        if (!AssetDatabase.IsValidFolder("Assets/Materials"))
        {
            AssetDatabase.CreateFolder("Assets", "Materials");
        }
        Material[] materials = new Material[colors.Length];
        for (int i = 0; i < colors.Length; i++)
        {
            Material mat = new Material(Shader.Find("Standard"));
            AssetDatabase.CreateAsset(mat, "Assets/Materials/leafMat" + i + ".mat");
            Color color;
            ColorUtility.TryParseHtmlString(colors[i], out color);
            mat.SetColor("_Color", color);
            materials[i] = mat;
        }

        const float FieldSizeHalf = 20;
        const int NumObjsPerSide = 40;
        const int jitterfactor = 3; //1/x is the fraction of the grid size in which to jitter
        const int anglerange = 45;
        const float level = 0.3f;
        const float vleafscale = 0.09f;
        const float hleafscale = 0.02f;

        const float X0 = -FieldSizeHalf;
        const float X1 = FieldSizeHalf;
        const float Z0 = -FieldSizeHalf;
        const float Z1 = FieldSizeHalf;

        // Assumes NumObjsPerSide is even (for now).
        const float DX = (X1 - X0) / (NumObjsPerSide + 1);
        const float DZ = (Z1 - Z0) / (NumObjsPerSide + 1);

        float x = X0 + DX;
        for (int ix = 0; ix < NumObjsPerSide; ix++)
        {
            float z = Z0 + DZ;
            for (int iz = 0; iz < NumObjsPerSide; iz++)
            {
                float jitterX = Random.Range(-DX / jitterfactor, DX / jitterfactor);
                float jitterZ = Random.Range(-DZ / jitterfactor, DZ / jitterfactor);
                float scaleX, scaleY, scaleZ;
                float rotX, rotY, rotZ;
                string name = "Leaf";
                PrimitiveType type;

                type = PrimitiveType.Plane;
                scaleX = hleafscale;
                scaleY = 1f;
                scaleZ = vleafscale;
                rotX = Random.Range(90-anglerange, 90+anglerange);
                rotY = Random.Range(-anglerange*4, anglerange*4);
                rotZ = Random.Range(90-anglerange, 90+anglerange);
                //name += "_leaf";

                Vector3 pos = new Vector3(x + jitterX, level, z + jitterZ);
                if (pos.magnitude > 0)
                {
                    GameObject obj = GameObject.CreatePrimitive(type);
                    obj.transform.position = pos;
                    obj.transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
                    obj.transform.localRotation = Quaternion.Euler(rotX, rotY, rotZ);
                    name += "_" + x.ToString() + "_" + z.ToString();
                    obj.name = name;

                    MeshCollider objcol = obj.GetComponent<MeshCollider>();
                    //objcol.enabled = false;

                    MeshRenderer mr = obj.GetComponent<MeshRenderer>();
                    int i = Random.Range(0, colors.Length);
                    mr.material = materials[i];
                }

                z += DZ;
            }

            x += DX;
        }

        // With the default `shadowDistance`, shadows from an object are visible only when
        // the camera is quite close to the object.
        QualitySettings.shadowDistance = 4 * FieldSizeHalf;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

#endif