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
        string[] colors = { "#a53600", "#b32db5", "#0072b2", "#908827", "#348e53", "#053cff"};
        if (!AssetDatabase.IsValidFolder("Assets/Materials"))
        {
            AssetDatabase.CreateFolder("Assets", "Materials");
        }
        Material[] materials = new Material[colors.Length];
        for (int i = 0; i < colors.Length; i++)
        {
            Material mat = new Material(Shader.Find("Standard"));
            AssetDatabase.CreateAsset(mat, "Assets/Materials/Mat" + i + ".mat");
            Color color;
            ColorUtility.TryParseHtmlString(colors[i], out color);
            mat.SetColor("_Color", color);
            materials[i] = mat;
        }

        const float FieldSizeHalf = 800;
        const float X0 = -FieldSizeHalf;
        const float X1 = FieldSizeHalf;
        const float Z0 = -FieldSizeHalf;
        const float Z1 = FieldSizeHalf;
        const int NumObjsPerSide = 8;

        // Assumes NumObjsPerSide is even (for  now).

        const float DX = (X1 - X0) / (NumObjsPerSide + 1);
        const float DZ = (Z1 - Z0) / (NumObjsPerSide + 1);

        float y = 0;

        float x = X0 + DX;
        for (int ix = 0; ix < NumObjsPerSide; ix++)
        {
            float z = Z0 + DZ;
            for (int iz = 0; iz < NumObjsPerSide; iz++)
            {
                float jitterX = Random.Range(-DX / 3, DX / 3);
                float jitterZ = Random.Range(-DZ / 3, DZ / 3);
                float scaleX, scaleY, scaleZ;
                string name = "Obstacle";
                PrimitiveType type;
                if (Random.value < 0.5)
                {
                    type = PrimitiveType.Cylinder;
                    scaleX = DX / 3;
                    scaleY = 70;
                    scaleZ = DZ / 3;
                    y = scaleY;
                    name += "_Cylinder";
                }
                else
                {
                    type = PrimitiveType.Cube;
                    scaleX = DX / 3;
                    scaleY = scaleX;
                    scaleZ = DZ / 3;
                    y = scaleY / 2;
                    name += "_Cube";
                }
                Vector3 pos = new Vector3(x + jitterX, y, z + jitterZ);
                if (pos.magnitude > 120)
                {
                    GameObject obj = GameObject.CreatePrimitive(type);
                    obj.transform.position = pos;
                    obj.transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
                    name += "_" + x.ToString() + "_" + z.ToString();
                    obj.name = name;

                    MeshRenderer mr = obj.GetComponent<MeshRenderer>();
                    int i = Random.Range(0, colors.Length);
                    mr.material = materials[i];
                }

                z += DZ;
            }

            x += DX;
        }

        GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plane.transform.position = Vector3.zero;
        plane.transform.localScale = new Vector3(FieldSizeHalf / 2, 1, FieldSizeHalf / 2);

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