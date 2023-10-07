using System;
using UnityEngine;

public class AnimateCylinderTexture : MonoBehaviour
{
    public float vRotDeg_per_sec = 60.0f;
    public float numElevationSteps = 10.0f;
    public int delaySeconds = 10;
    public float offsetTex = -90.0f;

    public int repeats = 3;

    private Material cylinderMaterial;
    private float elevation = 0;
    private int currentStep = 1;
    private float rotDir = 1.0f;

    //set up logging
    [Serializable]
    private class textureLogEntry : Janelia.Logger.Entry
    {
        public float xpos;
        public float ypos;

    };

    private textureLogEntry _currentLogEntry = new textureLogEntry();


    void Start()
    {
        cylinderMaterial = Resources.Load(Janelia.CylinderBackgroundResources.MaterialName, typeof(Material)) as Material;
        if (cylinderMaterial == null)
        {
            Debug.LogError("Could not load material'" + Janelia.CylinderBackgroundResources.MaterialName + "'");
        }

        //reset texture based on offset
        float x = offsetTex / 360.0f;
        float y = elevation;
        Vector2 offset = new Vector2(x, y);

        cylinderMaterial.SetTextureOffset("_MainTex", offset);

        //log values
        _currentLogEntry.xpos = x;
        _currentLogEntry.ypos = y;
        Janelia.Logger.Log(_currentLogEntry);
    }

    void Update()
    {
        if (cylinderMaterial & Time.time >= delaySeconds)
        {
            float dTime = Time.time - delaySeconds;

            float x = offsetTex/360.0f + dTime * rotDir * (vRotDeg_per_sec / 360.0f) % 1;

            //check if a round has been completed
            if (dTime * (vRotDeg_per_sec / 360.0f ) > currentStep)
            {
                if (currentStep % 2 == 1) // finished an uneven round --> first of two repeats
                {
                    rotDir = -1.0f; // now change direction
                }
                else // we finished the second repeat, change elevation and reset direction
                {
                    elevation += 1 / numElevationSteps;
                    rotDir = 1.0f;
                }
                currentStep += 1;

            }

            float y = elevation;
            Vector2 offset = new Vector2(x, y);

            //stop if done
            if ( currentStep <= repeats * 2 * numElevationSteps)
            {
                cylinderMaterial.SetTextureOffset("_MainTex", offset);

                //log values
                _currentLogEntry.xpos = x;
                _currentLogEntry.ypos = y;
                Janelia.Logger.Log(_currentLogEntry);
            }

        }
    }
}
