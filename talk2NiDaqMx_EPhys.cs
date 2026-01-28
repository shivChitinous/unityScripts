// Send signal using Janelia.NiDaqMx and a National Instruments (NI) DAQ device with the NI-DAQmx library.
using System;
using UnityEngine;
using static Janelia.NiDaqMx;
public class talk2NiDaqMxFlyRotation : MonoBehaviour
{
    public bool showEachWrite = false;
    private class Logs : Janelia.Logger.Entry
    {
        public double VoltageWritten;
        public double vm_mv;
        public double command_pa;
    };
    private Logs _currentLogEntry = new Logs();
    private void Start()
    {
        // Create parameters that are the default values except the specified value.
        _inputParams = new Janelia.NiDaqMx.InputParams
        {
            ChannelNames = new string[] { "ai0","ai1" }
        };
        Debug.Log("Scenario: '" + Janelia.SessionParameters.GetStringParameter("scenario") + "'");
        _readData = new double[_inputParams.SampleBufferSize];
        _writeData = new double[1];
        /**if (Janelia.SessionParameters.GetStringParameter("scenario") == "naive")
        {
            // TimeoutSecs * frames per second
            float defaultValue = Janelia.SessionParameters.GetFloatParameter("timeoutSecs") * 120.0f;
            int windowInFrames = (int)Janelia.SessionParameters.GetFloatParameter("window", defaultValue);
            Janelia.FicTracAverager.StartAveragingHeading(gameObject, windowInFrames);
        }**/
        if (!Janelia.NiDaqMx.CreateInputs(_inputParams))
        {
            Debug.LogError("Creating input failed");
            Debug.LogError(Janelia.NiDaqMx.GetLatestError());
            return;
        }
        // Create parameters that are the default values except the specified values.
        _outputParams = new Janelia.NiDaqMx.OutputParams
        {
            ChannelNames = new string[] { "ao0" },
            VoltageMin = -5,
            VoltageMax = 5
        };
        if (!Janelia.NiDaqMx.CreateOutputs(_outputParams))
        {
            Debug.LogError("Creating outputs failed");
            Debug.LogError(Janelia.NiDaqMx.GetLatestError());
            return;
        }
    }
    private void Update()
    {
        if (killSwitch == 1)
        {
            Debug.LogWarning("killSwitch caught!");
            return;
        }
        // Reading
        int numReadPerChannel = 0;
        if (!Janelia.NiDaqMx.ReadFromInputs(_inputParams, ref _readData, ref numReadPerChannel))
        {
            Debug.LogError($"Frame {Time.frameCount}: read from input failed");
            Debug.LogError(Janelia.NiDaqMx.GetLatestError());
        }
        else
        {
            if (numReadPerChannel > 0)
            {
                for (int i = 0; i < numReadPerChannel; i++)
                {
                    // Channel 2 (ai1)
                    int j = Janelia.NiDaqMx.IndexInReadBuffer(0, numReadPerChannel, i);
                    _currentLogEntry.vm_mv = _readData[j] * 10;
                }
                for (int i = 0; i < numReadPerChannel; i++)
                {
                    // Channel 3 (ai2)
                    int k = Janelia.NiDaqMx.IndexInReadBuffer(0, numReadPerChannel, i);
                    _currentLogEntry.command_pa = _readData[k];
                    Janelia.Logger.Log(_currentLogEntry);
                }
            }
            else
            {
                Debug.LogWarning($"Frame {Time.frameCount}: unexpectedly, read {numReadPerChannel} values");
            }
        }
        // Writing
        int numWritten = 0;
        double writeValueModulator = (_outputParams.VoltageMax - _outputParams.VoltageMin) / 360;
        //_writeData[0] = transform.rotation.eulerAngles.y * 0.999 * writeValueModulator + _outputParams.VoltageMin ;
        if (odd)
        {
            _writeData[0] = _outputParams.VoltageMax;
            odd = false;
        }
        else
        {
            _writeData[0] = _outputParams.VoltageMin;
            odd = true;
        }
        /**double safeheading = 180;
       // Scenario logic
        if (Janelia.SessionParameters.GetStringParameter("scenario") != "naive")
        {
            //double estimatedAnglePreference = (Janelia.FicTracAverager.GetAverageHeading(gameObject) + 360) % 360;
            safeheading = (estimatedAnglePreference - 180 + 360) % 360;
        }
        //double safezone = 135;
        //double minsafe = (safeheading - safezone + 360) % 360;
        //double maxsafe = (safeheading + safezone + 360) % 360;
        _writeData[1] = _outputParams.VoltageMin;
        if (Janelia.SessionParameters.GetStringParameter("scenario") != "train")
        {
            _writeData[1] = _outputParams.VoltageMin;
        }
        else if ((maxsafe > minsafe && transform.rotation.eulerAngles.y >= minsafe && transform.rotation.eulerAngles.y <= maxsafe) ||
                 (maxsafe <= minsafe && (transform.rotation.eulerAngles.y >= minsafe || transform.rotation.eulerAngles.y <= maxsafe)))
        {
            _writeData[1] = _outputParams.VoltageMin;
        }
        else
        {
            _writeData[1] = _outputParams.VoltageMax;
        } **/
        int expectedNumWritten = _writeData.Length; 
        if (!Janelia.NiDaqMx.WriteToOutputs(_outputParams, _writeData, ref numWritten))
        {
            Debug.LogError($"Frame {Time.frameCount}: write to outputs failed");
            Debug.LogError(Janelia.NiDaqMx.GetLatestError());
        }
        else
        {
            _currentLogEntry.VoltageWritten = _writeData[0];
            Janelia.Logger.Log(_currentLogEntry);
            if (showEachWrite)
            {
                Debug.Log($"Frame {Time.frameCount}: wrote {numWritten} value(s): {_writeData}");
            }
        }
    }
    private void OnDestroy()
    {
        Debug.Log("OnDestroy called");
        int expectedNumWritten = _writeData.Length;
        int numWritten = 0;
        killSwitch = 1;
        _writeData[0] = _outputParams.VoltageMin;
        //_writeData[1] = _outputParams.VoltageMin;
        //_writeData[2] = _outputParams.VoltageMin;
        if (!Janelia.NiDaqMx.WriteToOutputs(_outputParams, _writeData, ref numWritten))
        {
            Debug.LogError($"Frame {Time.frameCount}: write to outputs failed");
            Debug.LogError(Janelia.NiDaqMx.GetLatestError());
        }
        else if (numWritten != expectedNumWritten)
        {
            Debug.LogWarning($"Frame {Time.frameCount}: unexpectedly, wrote {numWritten} values");
        }
        Janelia.NiDaqMx.OnDestroy();
    }
    public double killSwitch = 0;
    public bool odd = true;
    private Janelia.NiDaqMx.InputParams _inputParams;
    double[] _readData;
    double[] _writeData;
    private Janelia.NiDaqMx.OutputParams _outputParams;
}