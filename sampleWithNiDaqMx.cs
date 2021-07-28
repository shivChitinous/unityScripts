// Read photodiode (PD) signal using Janelia.NiDaqMx and a
// National Instruments (NI) DAQ device with the NI-DAQmx library.

using System;
using UnityEngine;

public class sampleWithNiDaqMx : MonoBehaviour
{

    [Serializable]
    private class photoDiodeLogEntry : Janelia.Logger.Entry
    {
        public double tracePD;
        public double imgFrameTrigger;
    };

    private photoDiodeLogEntry _currentLogEntry = new photoDiodeLogEntry();

    private void Start()
    {
        // Create parameters that are all the default values.
        _inputParams = new Janelia.NiDaqMx.InputParams();

        /*
        _inputParams2 = new Janelia.NiDaqMx.InputParams
        {
            ChannelName = "ai1"
        };
        */

        // Create parameters that are the default values except the specified value.
        //_outputParams = new Janelia.NiDaqMx.OutputParams();


        if (!Janelia.NiDaqMx.CreateInput(_inputParams))
        {
            Debug.Log("Creating input failed");
            Debug.Log(Janelia.NiDaqMx.GetLatestError());
            return;
        }

        /*
        if (!Janelia.NiDaqMx.CreateInput(_inputParams2))
        {
            Debug.Log("Creating input failed");
            Debug.Log(Janelia.NiDaqMx.GetLatestError());
            return;
        }
        */

        //if (!Janelia.NiDaqMx.CreateOutput(_outputParams))
        //{
        //    Debug.Log("Creating output failed");
        //    Debug.Log(Janelia.NiDaqMx.GetLatestError());
        //    return;
        //}

        _readData = new double[_inputParams.sampleBufferSize];
        //_readData2 = new double[_inputParams2.sampleBufferSize];
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
                    _currentLogEntry.tracePD = _readData[i];
                    Janelia.Logger.Log(_currentLogEntry);
                }

            }
            else
            {
                Debug.Log("Frame " + Time.frameCount + ": unexpectedly, read " + numRead + " values");
            }
        }

    }

    private void OnDestroy()
    {
        Janelia.NiDaqMx.OnDestroy();
    }

    private Janelia.NiDaqMx.InputParams _inputParams;
    //private Janelia.NiDaqMx.InputParams _inputParams2;
    double[] _readData;
    //double[] _readData2;
}