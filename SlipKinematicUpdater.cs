using System;
using UnityEngine;
namespace Janelia
{
    public class SlipKinematicUpdater : KinematicSubject.IKinematicUpdater
    {
        public string ficTracServerAddress = "127.0.0.1";
        public int ficTracServerPort = 2000;
        public float ficTracBallRadius = 0.5f;
        public int smoothingCount = 3;
        public int ficTracBufferCount = 240;
        public bool logFicTracMessages = false;
        public float slipInterval = 5f; // Duration in seconds to enable/disable updating
        private FicTracUpdater _ficTracUpdater;
        private bool _isUpdating = true;
        private float _nextToggleTime;
        private Vector3 _deltaRotationVectorLabUpdated = Vector3.zero;
        public void Start()
        {
            // Initialize FicTracUpdater with the necessary parameters.
            _ficTracUpdater = new FicTracUpdater
            {
                ficTracServerAddress = ficTracServerAddress,
                ficTracServerPort = ficTracServerPort,
                ficTracBallRadius = ficTracBallRadius,
                smoothingCount = smoothingCount,
                ficTracBufferCount = ficTracBufferCount,
                logFicTracMessages = logFicTracMessages
            };
            _ficTracUpdater.Start();
            _nextToggleTime = Time.time + slipInterval;
        }
        public void Update()
        {
            // Toggle updating state every slipInterval seconds
            if (Time.time >= _nextToggleTime)
            {
                _isUpdating = !_isUpdating;
                _nextToggleTime = Time.time + slipInterval;
                // Log state for debugging
                Debug.Log("SlipKinematicUpdater is now " + (_isUpdating ? "active" : "inactive"));
            }
            // Update only if currently active
            if (_isUpdating)
            {
                _ficTracUpdater.Update();
                _deltaRotationVectorLabUpdated = _ficTracUpdater.Translation() ?? Vector3.zero;
            }
            else
            {
                // Reset the updated vector to avoid unintended movement while inactive
                _deltaRotationVectorLabUpdated = Vector3.zero;
            }
        }
        public Vector3? Translation()
        {
            float s = ficTracBallRadius;
            float forward = _deltaRotationVectorLabUpdated[1] * s;
            float sideways = _deltaRotationVectorLabUpdated[0] * s;
            return new Vector3(forward, 0, sideways);
        }
        public Vector3? RotationDegrees()
        {
            float s = Mathf.Rad2Deg;
            float heading = _deltaRotationVectorLabUpdated[2] * s;
            return new Vector3(0, -heading, 0);
        }
        public void OnDisable()
        {
            _ficTracUpdater.OnDisable();
        }
    }
}