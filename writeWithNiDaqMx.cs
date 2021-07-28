// Send signal using Janelia.NiDaqMx and a
// National Instruments (NI) DAQ device with the NI-DAQmx library.

using System;
using UnityEngine;

public class writeWithNiDaqMx : MonoBehaviour
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
        // Create parameters that are all the default values.
        _inputParams = new Janelia.NiDaqMx.InputParams
        {
            ChannelName = "ai1"
        };

        // Create parameters that are the default values except the specified value.
        _outputParams = new Janelia.NiDaqMx.OutputParams
        {
            ChannelName = "ao0"
        };

        // Create parameters that are the default values except the specified value.
        //_outputParams = new Janelia.NiDaqMx.OutputParams();


        if (!Janelia.NiDaqMx.CreateInput(_inputParams))
        {
            Debug.Log("Creating input failed");
            Debug.Log(Janelia.NiDaqMx.GetLatestError());
            return;
        }

        if (!Janelia.NiDaqMx.CreateOutput(_outputParams))
        {
            Debug.Log("Creating output failed");
            Debug.Log(Janelia.NiDaqMx.GetLatestError());
            return;
        }

        _readData = new double[_inputParams.sampleBufferSize];


        // initiate trigger to microscope
        int numWritten = 0;

        int expectedNumWritten = 1;
        double writeValue =  _outputParams.VoltageMax;
        //writeValue = 0.1 * writeValue;
        if (!Janelia.NiDaqMx.WriteToOutput(_outputParams, writeValue, ref numWritten))

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

        int numRead = 0;
        if (!Janelia.NiDaqMx.ReadFromInput(_inputParams, ref _readData, ref numRead))
        {
            Debug.Log("Frame " + Time.frameCount + ": read from input failed");
            Debug.Log(Janelia.NiDaqMx.GetLatestError());
        }
        else
        {
            if (numRead > 0)
            {
                // log read values
                for (int i = 0; i < numRead; i++)
                {
                    //_currentLogEntry.imgFrameTrigger = _readData[i];
                    _currentLogEntry.tracePD = _readData[i];
                    Janelia.Logger.Log(_currentLogEntry);
                }

            }
            else
            {
                Debug.Log("Frame " + Time.frameCount + ": unexpectedly, read " + numRead + " values");
            }
        }



        // Writing

        if (Input.anyKeyDown)
        {
            int numWritten = 0;

            int expectedNumWritten = 1;
            double writeValue = (_iWrite % 2 == 0) ? _outputParams.VoltageMax : _outputParams.VoltageMin;
            //writeValue = 0.1 * writeValue;
            if (!Janelia.NiDaqMx.WriteToOutput(_outputParams, writeValue, ref numWritten))

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