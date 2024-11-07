using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace Janelia
{
    public class talk2NiDaqMx : MonoBehaviour
    {
        public bool showEachWrite = false;

        [Serializable]
        private class photoDiodeLogEntry : Logger.Entry
        {
            public double tracePD = 0.0;
            public double imgFrameTrigger;
        }

        private photoDiodeLogEntry _currentLogEntry = new photoDiodeLogEntry();

        private void Start()
        {
            // Create input and output parameters
            _inputParams = new NiDaqMx.InputParams
            {
                ChannelNames = new string[] { "ai0", "ai1" }
            };

            _outputParams = new NiDaqMx.OutputParams()
            {
                ChannelNames = new string[] { "ao0" },
                VoltageMin = -5,
                VoltageMax = 5
            };

            // Initialize inputs and outputs
            if (!NiDaqMx.CreateInputs(_inputParams))
            {
                Debug.LogError("Creating input 0 failed");
                Debug.LogError(NiDaqMx.GetLatestError());
                return;
            }

            if (!NiDaqMx.CreateOutputs(_outputParams))
            {
                Debug.LogError("Creating output failed");
                Debug.LogError(NiDaqMx.GetLatestError());
                return;
            }

            _readData = new double[_inputParams.SampleBufferSize];

            // Initial trigger to microscope
            int numWritten = 0;
            double writeValue = _outputParams.VoltageMax;
            if (!NiDaqMx.WriteToOutputs(_outputParams, writeValue, ref numWritten))
            {
                Debug.LogError("Initial write to output failed");
                Debug.LogError(NiDaqMx.GetLatestError());
            }
            else if (showEachWrite)
            {
                Debug.Log("Frame " + Time.frameCount + ": wrote " + numWritten + " value(s): " + writeValue);
            }

            // Start coroutine to read data from inputs
            StartCoroutine(ReadDataCoroutine());
        }

        private void Update()
        {
            // Writing when any key is pressed
            if (Input.anyKeyDown)
            {
                int numWritten = 0;
                double writeValue = (_iWrite % 2 == 0) ? _outputParams.VoltageMax : _outputParams.VoltageMin;

                if (!NiDaqMx.WriteToOutputs(_outputParams, writeValue, ref numWritten))
                {
                    Debug.LogError("Write to output failed on key press");
                    Debug.LogError(NiDaqMx.GetLatestError());
                }
                else if (showEachWrite)
                {
                    Debug.Log("Frame " + Time.frameCount + ": wrote " + numWritten + " value(s): " + writeValue);
                }

                _iWrite++;
            }
        }

        private IEnumerator ReadDataCoroutine()
        {
            while (true)
            {
                int numReadPerChannel = 0;
                Task<bool> readTask = NiDaqMx.ReadFromInputsAsync(_inputParams, _readData, numReadPerChannel);

                yield return new WaitUntil(() => readTask.IsCompleted);

                if (readTask.Result && numReadPerChannel > 0)
                {
                    for (int i = 0; i < numReadPerChannel; i++)
                    {
                        // Log data for channel ai0
                        int j = NiDaqMx.IndexInReadBuffer(0, numReadPerChannel, i);
                        _currentLogEntry.tracePD = _readData[j];

                        // Log data for channel ai1
                        int k = NiDaqMx.IndexInReadBuffer(1, numReadPerChannel, i);
                        _currentLogEntry.imgFrameTrigger = _readData[k];

                        Logger.Log(_currentLogEntry);
                    }
                }
                else
                {
                    Debug.LogWarning("Read failed or returned zero values");
                    if (!readTask.Result)
                    {
                        Debug.LogError(NiDaqMx.GetLatestError());
                    }
                }

                // Small delay to prevent too frequent polling
                yield return new WaitForSeconds(0.05f); // Adjust delay as needed based on your requirements
            }
        }

        private void OnDestroy()
        {
            NiDaqMx.OnDestroy();
        }

        private NiDaqMx.InputParams _inputParams;
        private double[] _readData;

        private NiDaqMx.OutputParams _outputParams;
        private int _iWrite = 0;
    }
}