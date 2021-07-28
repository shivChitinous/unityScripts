using UnityEngine;
using System;

public class moveCylinder : MonoBehaviour
{
    public float rotSpeed;
    public float numRotations;
    public float elevationStepSize;

    private float azimuth = 0;
    private float elevation = 0;
    private int round = 0;

    private Vector3 currentEulerAngles;

    [Serializable]
    private class cylinderLogEntry : Janelia.Logger.Entry
    {
        public double azimuthLog;
        public double elevationLog;
        public int roundLog;
    };

    private cylinderLogEntry _currentLogEntry = new cylinderLogEntry();

    // Start is called before the first frame update
    void Start()
    {
    
    }

    // Update is called once per frame
    void Update()
    {
        //if (round < numRotations)
        //{
        currentEulerAngles = new Vector3(0, azimuth, 0);
        transform.localEulerAngles = currentEulerAngles;
        azimuth += rotSpeed * Time.deltaTime; // use dt to convert these into actual positions.

        if ( (int) (azimuth/360) > round)
        {
            round += 1;
            Debug.Log("next round");
            elevation += elevationStepSize;
            transform.Translate(0, elevation, 0); // after 1 round jump in elevation
        }

        _currentLogEntry.azimuthLog = azimuth % 360;
        _currentLogEntry.elevationLog = elevation;
        _currentLogEntry.roundLog = round;

        Janelia.Logger.Log(_currentLogEntry);

    }
}
