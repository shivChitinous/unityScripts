using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

public class talk2MccDaq : MonoBehaviour
{
    public bool showEachRead = false;

    [Serializable]
    private class TemperatureLogEntry : Janelia.Logger.Entry
    {
        public float temperature = 0.0f;
    };

    private TemperatureLogEntry _currentLogEntry = new TemperatureLogEntry();
    private Janelia.MccDaq.InputParams _inputParams;
    private bool _isReading = false; // Flag to prevent overlapping read attempts

    private void Start()
    {
        // Create input parameters for the MCC DAQ
        _inputParams = new Janelia.MccDaq.InputParams
        {
            BoardNum = 0,
            Channel = 0,
            Thermocouple = Janelia.MccDaq.ThermocoupleType.K,
            BufferSize = 1000
        };

        // Initialize the DAQ device
        if (!Janelia.MccDaq.Initialize(_inputParams))
        {
            Debug.Log("Initialization failed");
            Debug.Log(Janelia.MccDaq.GetLatestError());
            return;
        }

        // Start the coroutine to periodically start temperature readings
        StartCoroutine(ReadTemperatureCoroutine());
    }

    private IEnumerator ReadTemperatureCoroutine()
    {
        while (true)
        {
            if (!_isReading) // Only start a new read if one is not already in progress
            {
                ReadTemperatureAsync();
            }

            // Wait for 1 second before attempting the next reading
            yield return new WaitForSeconds(1.0f);
        }
    }

    private async void ReadTemperatureAsync()
    {
        _isReading = true;

        // Run the temperature reading in a separate thread
        await Task.Run(() =>
        {
            if (!Janelia.MccDaq.ReadFromInputs(_inputParams))
            {
                Debug.Log("Read from input failed: " + Janelia.MccDaq.GetLatestError());
            }
        });

        // Retrieve the data on the main thread
        int numReadings = Janelia.MccDaq.GetNumReadings(_inputParams);
        float[] temperatureBuffer = Janelia.MccDaq.GetTemperatureBuffer(_inputParams);

        if (numReadings > 0)
        {
            for (int i = 0; i < numReadings; i++)
            {
                _currentLogEntry.temperature = temperatureBuffer[i];
                Janelia.Logger.Log(_currentLogEntry);

                if (showEachRead)
                {
                    Debug.Log("Temperature reading " + (i + 1) + ": " + temperatureBuffer[i] + " °C");
                }
            }
        }

        _isReading = false;
    }
}
