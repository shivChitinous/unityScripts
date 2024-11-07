// Send signal using Janelia.NiDaqMx and a National Instruments (NI) DAQ device with the NI-DAQmx library.

using System;
using UnityEngine;

public class talk2NiDaqMx : MonoBehaviour
{
    public bool showEachWrite = false;

    [Serializable]
    private class photoDiodeLogEntry : Janelia.Logger.Entry
    {
        public double tracePD = 0.0;
        public double imgFrameTrigger;
    };

    private photoDiodeLogEntry _currentLogEntry = new photoDiodeLogEntry();

    private void Start()
    {
        // Create parameters that are the default values except the specified value.
        _inputParams = new Janelia.NiDaqMx.InputParams
        {
            ChannelNames = new string[] { "ai0", "ai1" }
        };

        // Create parameters that are the default values except the specified values.
        _outputParams = new Janelia.NiDaqMx.OutputParams()
        {
            ChannelNames = new string[] { "ao0" },
            VoltageMin = -5,
            VoltageMax = 5
        };


        // To create parameters that are all the default values, use the following:
        // _outputParams = new Janelia.NiDaqMx.OutputParams();


        if (!Janelia.NiDaqMx.CreateInputs(_inputParams))
        {
            Debug.Log("Creating input 0 failed");
            Debug.Log(Janelia.NiDaqMx.GetLatestError());
            return;
        }

        if (!Janelia.NiDaqMx.CreateOutputs(_outputParams))
        {
            Debug.Log("Creating output failed");
            Debug.Log(Janelia.NiDaqMx.GetLatestError());
            return;
        }

        _readData = new double[_inputParams.SampleBufferSize];


        // initiate trigger to microscope
        int numWritten = 0;

        int expectedNumWritten = 1;
        double writeValue = _outputParams.VoltageMax;
        //writeValue = 0.1 * writeValue;
        if (!Janelia.NiDaqMx.WriteToOutputs(_outputParams, writeValue, ref numWritten))

        {
            Debug.Log("Frame " + Time.frameCount + ": write to output failed");
            Debug.Log(Janelia.NiDaqMx.GetLatestError());
        }
        if (numWritten != expectedNumWritten)
        {
            Debug.Log("Frame " + Time.frameCount + ": unexpectedly, wrote " + numWritten + " values");
        }
        else if (showEachWrite)
        {
            Debug.Log("Frame " + Time.frameCount + ": wrote " + numWritten + " value(s): " + writeValue);
        }


    }

    private void Update()
    {

        // Reading
        int numReadPerChannel = 0;
        if (!Janelia.NiDaqMx.ReadFromInputs(_inputParams, ref _readData, ref numReadPerChannel))
        {
            Debug.Log("Frame " + Time.frameCount + ": read from input failed");
            Debug.Log(Janelia.NiDaqMx.GetLatestError());
        }
        else
        {
            if (numReadPerChannel > 0)
            {
                // log read values
                for (int i = 0; i < numReadPerChannel; i++)
                {
                    //channel 1 (ai0)
                    int j = Janelia.NiDaqMx.IndexInReadBuffer(0, numReadPerChannel, i);
                    _currentLogEntry.tracePD = _readData[j];

                    //channel 2 (ai1)
                    int k = Janelia.NiDaqMx.IndexInReadBuffer(1, numReadPerChannel, i);
                    _currentLogEntry.imgFrameTrigger = _readData[k];

                    Janelia.Logger.Log(_currentLogEntry);

                }
            }
            else
            {
                Debug.Log("Frame " + Time.frameCount + ": unexpectedly, read " + numReadPerChannel + " values");
            }
        }


        // Writing

        if (Input.anyKeyDown)
        {
            int numWritten = 0;

            int expectedNumWritten = 1;
            double writeValue = (_iWrite % 2 == 0) ? _outputParams.VoltageMax : _outputParams.VoltageMin;
            //writeValue = 0.1 * writeValue;
            if (!Janelia.NiDaqMx.WriteToOutputs(_outputParams, writeValue, ref numWritten))

            {
                Debug.Log("Frame " + Time.frameCount + ": write to output failed");
                Debug.Log(Janelia.NiDaqMx.GetLatestError());
            }
            if (numWritten != expectedNumWritten)
            {
                Debug.Log("Frame " + Time.frameCount + ": unexpectedly, wrote " + numWritten + " values");
            }
            else if (showEachWrite)
            {
                Debug.Log("Frame " + Time.frameCount + ": wrote " + numWritten + " value(s): " + writeValue);
            }
            _iWrite++;
        }

    }

    private void OnDestroy()
    {
        Janelia.NiDaqMx.OnDestroy();
    }

    private Janelia.NiDaqMx.InputParams _inputParams;
    double[] _readData;

    private Janelia.NiDaqMx.OutputParams _outputParams;
    int _iWrite = 0;
}