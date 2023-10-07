using System;
using UnityEngine;

public class SweepPanoramaOpenLoop : MonoBehaviour
{
    public float vRotDeg_per_sec_1 = 10.0f;
    public float vRotDeg_per_sec_2 = 30.0f;
    public float vRotDeg_per_sec_3 = 60.0f;
    public int delaySeconds = 5;

    public int repeats = 2;

    private Material cylinderMaterial;
    private float timeInSet = 0.0f;
    private float setStart = 0.0f;
    private int currentSet = 0;
    private int currentRound = 1;
    private float rotDir = 1.0f;
    private float vRotDeg_per_sec;

    //set up logging
    [Serializable]
    private class textureLogEntry : Janelia.Logger.Entry
    {
        public float xpos;
        public float speed;
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
        float x = 0.0f;
        Vector2 offset = new Vector2(x, 0);
        
        vRotDeg_per_sec = vRotDeg_per_sec_1;

        cylinderMaterial.SetTextureOffset("_MainTex", offset);

        //log values
        _currentLogEntry.xpos = x;
        _currentLogEntry.speed = vRotDeg_per_sec;
        Janelia.Logger.Log(_currentLogEntry);
    }

    void Update()
    {
        if (cylinderMaterial & Time.time >= delaySeconds)
        {
            float dTime = Time.time - delaySeconds;

            //check if a set has been completed
            if (currentSet == 0 & dTime > 0){
                // first rotation set
                currentSet = 1;
                vRotDeg_per_sec = vRotDeg_per_sec_1;
                setStart = Time.time;
                currentRound = 1;
                rotDir = 1.0f;
                print("#1");
            }
            else if (currentSet == 1 & (timeInSet * (vRotDeg_per_sec / 360.0f )) >= repeats*2 ){
                // second rotation set
                currentSet = 2;
                vRotDeg_per_sec = vRotDeg_per_sec_2;
                setStart = Time.time;
                currentRound = 1;
                rotDir = 1.0f;
                print("#2");
            }
            else if (currentSet == 2 & (timeInSet * (vRotDeg_per_sec / 360.0f )) >= repeats*2 ){
                // third rotation set
                currentSet = 3;
                vRotDeg_per_sec = vRotDeg_per_sec_3;
                setStart = Time.time;
                currentRound = 1;
                rotDir = 1.0f;
                print("#3");
            }
            else if (currentSet == 3 & (timeInSet * (vRotDeg_per_sec / 360.0f )) >= repeats*2 ){
                // third rotation set
                currentSet = 4;
                vRotDeg_per_sec = 0;
                setStart = Time.time;
                currentRound = 1;
                rotDir = 1.0f;
                print("end");
            }

            if (currentSet > 0 & currentSet < 4){

                timeInSet = Time.time - setStart;
                // compute new texture position
                float x = timeInSet * rotDir * (vRotDeg_per_sec / 360.0f) % 1;

                if ( timeInSet * (vRotDeg_per_sec / 360.0f ) > currentRound)
                {
                    if (currentRound % 2 == 1) // finished an uneven round --> first of two repeats
                    {
                        rotDir = -1.0f; // now change direction
                    }
                    else // we finished the second repeat, reset direction
                    {
                        rotDir = 1.0f;
                    }
                    currentRound += 1;
                }

                // update offset position
                Vector2 offset = new Vector2(x, 0);
                cylinderMaterial.SetTextureOffset("_MainTex", offset);

                //log values
                _currentLogEntry.xpos = x;
                _currentLogEntry.speed = vRotDeg_per_sec;
                Janelia.Logger.Log(_currentLogEntry);
            }

        }
    }
}
