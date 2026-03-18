using System;
using UnityEngine;
using UnityEngine.Animations;

public class AnimateCylinderTextureWithClosedLoop : MonoBehaviour
{
    public float[] vRotDeg_per_sec;

    public int[] sweepRepeatVec;

    public float numElevationSteps = 10.0f;

    public float sweepDelaySeconds = 0;
    public float offsetTex = -90.0f;

    public float offsetEl= 0.1f;

    public int closedLoopDurationSeconds = 10;
    public bool alternateStartDirection = false;

    private int repeats = 1; // use repeating velocities to ensure repeats
    private Material cylinderMaterial;
    private float elevation;
    private int currentStep = 1;
    private float rotDir = 1.0f;
    private int vel = 0;
    private float waitTime = 0;
    private float sweepWaitTime = 0;
    private bool inSweepDelay = false;
    private float totalSweepDelayTime = 0;
    private bool inClosedLoop = true; // Start in closed loop
    private float closedLoopStartTime = 0;
    private bool isVelocityTransitionCL = false; // Distinguishes velocity-transition CL from between-sweep CL

    public float cylinderDeg = 360.0f;

    private RotationConstraint _rotationConstraint;

    //set up logging
    [Serializable]
    private class textureLogEntry : Janelia.Logger.Entry
    {
        public float xpos;
        public float ypos;
        public bool isClosedLoop;
    };

    private textureLogEntry _currentLogEntry = new textureLogEntry();


    void Start()
    {
        cylinderMaterial = Resources.Load(Janelia.CylinderBackgroundResources.MaterialName, typeof(Material)) as Material;

        if (cylinderMaterial == null)
        {
            Debug.LogError("Could not load material'" + Janelia.CylinderBackgroundResources.MaterialName + "'");
        }

        _rotationConstraint = FindObjectOfType<RotationConstraint>();
        if (_rotationConstraint == null)
        {
            Debug.LogError("No RotationConstraint found in scene");
        }
        else
        {
            Debug.Log("Found RotationConstraint on: " + _rotationConstraint.gameObject.name
                + ", constraintActive=" + _rotationConstraint.constraintActive);
        }

        //reset texture based on offset
        elevation = offsetEl;
        float x = offsetTex / 360.0f;
        float y = elevation;

        Vector2 offset = new Vector2(x, y);

        cylinderMaterial.SetTextureOffset("_MainTex", offset);

        // Start in closed loop: relax rotation constraint
        isVelocityTransitionCL = true; // Initial CL uses velocity-transition path (sets waitTime on end)
        closedLoopStartTime = Time.time;
        if (_rotationConstraint != null)
        {
            _rotationConstraint.constraintActive = false;
            Debug.Log("Closed loop ON (start): constraintActive set to false");
        }

        //log values
        _currentLogEntry.xpos = x;
        _currentLogEntry.ypos = y;
        _currentLogEntry.isClosedLoop = true;
        Janelia.Logger.Log(_currentLogEntry);
    }

    void Update()
    {
        // Handle closed-loop period
        if (inClosedLoop)
        {
            if (Time.time >= closedLoopStartTime + closedLoopDurationSeconds)
            {
                inClosedLoop = false;
                if (_rotationConstraint != null)
                {
                    _rotationConstraint.constraintActive = true;
                    Debug.Log("Closed loop OFF: constraintActive set to true");
                }

                if (isVelocityTransitionCL)
                {
                    // Velocity transition CL: reset waitTime for new velocity
                    waitTime = Time.time;
                }
                else
                {
                    // Between-pair CL: absorb CL duration into totalSweepDelayTime
                    // so dTime calculation skips over the CL period
                    totalSweepDelayTime += Time.time - closedLoopStartTime;

                    // Enter sweep delay before next sweep (CL → delay → next CCW)
                    if (sweepDelaySeconds > 0)
                    {
                        inSweepDelay = true;
                        sweepWaitTime = Time.time;
                    }
                }
            }
            return;
        }

        // All velocities done
        if (vel >= vRotDeg_per_sec.Length)
        {
            return;
        }

        //check if a velocity has been completed
        if (currentStep > repeats * 2 * sweepRepeatVec[vel] * numElevationSteps)
        {
            vel+=1;
            currentStep = 1;
            elevation = offsetEl;
            rotDir = 1.0f;
            totalSweepDelayTime = 0;
            inSweepDelay = false;

            // Enter closed-loop mode (velocity transition): freeze texture, relax rotation constraint
            inClosedLoop = true;
            isVelocityTransitionCL = true;
            closedLoopStartTime = Time.time;
            if (_rotationConstraint != null)
            {
                _rotationConstraint.constraintActive = false;
                Debug.Log("Closed loop ON (vel transition): constraintActive set to false, vel=" + vel);
            }

            // Reset texture to original position
            float x = offsetTex / 360.0f;
            float y = offsetEl;
            cylinderMaterial.SetTextureOffset("_MainTex", new Vector2(x, y));

            _currentLogEntry.xpos = x;
            _currentLogEntry.ypos = y;
            _currentLogEntry.isClosedLoop = true;
            Janelia.Logger.Log(_currentLogEntry);
            return;
        }

        // Wait out sweep delay between individual sweeps
        if (inSweepDelay)
        {
            if (Time.time >= sweepWaitTime + sweepDelaySeconds)
            {
                inSweepDelay = false;
                totalSweepDelayTime += Time.time - sweepWaitTime;
            }
            return;
        }

        if (cylinderMaterial & Time.time >= (waitTime+sweepDelaySeconds))
        {

            float dTime = Time.time - (waitTime+sweepDelaySeconds) - totalSweepDelayTime;
            float x = offsetTex/cylinderDeg + dTime * rotDir * (vRotDeg_per_sec[vel] / cylinderDeg) % 1;

            //check if a round has been completed
            if (dTime * (vRotDeg_per_sec[vel] / 360.0f ) > currentStep)
            {
                if (currentStep % (2*sweepRepeatVec[vel]) == 0) // we finished the nth repeat, change elevation and reset direction
                {
                    elevation += (1-offsetEl) / numElevationSteps;
                    // nextPairIdx = currentStep/2 (0-based index of the upcoming pair)
                    rotDir = (alternateStartDirection && (currentStep / 2) % 2 == 1) ? -1.0f : 1.0f;
                }
                else if (currentStep % 2 == 0) // finished an even round --> second of two repeats (pair complete)
                {
                    // nextPairIdx = currentStep/2
                    rotDir = (alternateStartDirection && (currentStep / 2) % 2 == 1) ? -1.0f : 1.0f;
                }
                else // finished an uneven round --> first of two repeats (mid-pair)
                {
                    // currentPairIdx = (currentStep-1)/2
                    // On alternating odd pairs the first sweep is CW, so mid-pair → next is CCW
                    rotDir = (alternateStartDirection && ((currentStep - 1) / 2) % 2 == 1) ? 1.0f : -1.0f;
                }
                currentStep += 1;

                // After a completed pair (CW done, currentStep now odd):
                // enter CL immediately, unless it's the last pair of the velocity
                // (which is handled by the velocity-transition check at the top of Update).
                bool pairComplete = (currentStep % 2 == 1);
                bool lastPairOfVel = (currentStep > repeats * 2 * sweepRepeatVec[vel] * numElevationSteps);

                if (pairComplete && !lastPairOfVel)
                {
                    inClosedLoop = true;
                    isVelocityTransitionCL = false;
                    closedLoopStartTime = Time.time;
                    if (_rotationConstraint != null)
                    {
                        _rotationConstraint.constraintActive = false;
                        Debug.Log("Closed loop ON (between pairs): constraintActive set to false");
                    }
                    return;
                }

                // Mid-pair (CCW done) or last pair: normal sweep delay
                if (sweepDelaySeconds > 0)
                {
                    inSweepDelay = true;
                    sweepWaitTime = Time.time;
                    return;
                }

            }

            float y = elevation;
            Vector2 offset = new Vector2(x, y);

            //stop if done
            if (currentStep*vel <= (repeats * 2 * numElevationSteps * sweepRepeatVec[sweepRepeatVec.Length-1] * vRotDeg_per_sec.Length))
            {

                cylinderMaterial.SetTextureOffset("_MainTex", offset);

                //log values
                _currentLogEntry.xpos = x;
                _currentLogEntry.ypos = y;
                _currentLogEntry.isClosedLoop = false;
                Janelia.Logger.Log(_currentLogEntry);
            }

        }

    }

}
