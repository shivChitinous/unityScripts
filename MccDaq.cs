using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;

namespace Janelia
{
    public static class MccDaq
    {
        // Enumeration for thermocouple types
        public enum ThermocoupleType
        {
            J = 0,
            K,
            T,
            E,
            R,
            S,
            B,
            N,
            Thermistor,
            SemiCond
        }

        // Enumeration for error codes returned by the MCC Universal Library functions
        public enum ErrorCode
        {
            NoErrors = 0,
            BadChannel = 1,
            OutOfMemory = 2,
            // Add more error codes as per UL documentation
        }

        // Importing the MCC Universal Library functions from the 64-bit DLL
        [DllImport("cbw64.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int cbDeclareRevision(ref float revisionNumber);

        [DllImport("cbw64.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int cbErrHandling(int errorReporting, int errorHandling);

        [DllImport("cbw64.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int cbTIn(int boardNum, int channel, int scale, ref float tempValue, int options);

        // Input parameters for temperature readings
        public class InputParams
        {
            internal int boardNum = 0;
            public int BoardNum
            {
                get { return boardNum; }
                set { Restrict(); boardNum = value; }
            }

            internal int channel = 0;
            public int Channel
            {
                get { return channel; }
                set { Restrict(); channel = value; }
            }

            internal ThermocoupleType thermocoupleType = ThermocoupleType.J;
            public ThermocoupleType Thermocouple
            {
                get { return thermocoupleType; }
                set { Restrict(); thermocoupleType = value; }
            }

            internal int bufferSize = 1000;
            public int BufferSize
            {
                get { return bufferSize; }
                set { Restrict(); bufferSize = value; }
            }

            internal double timeoutSecs = 10.0;
            public double TimeoutSecs
            {
                get { return timeoutSecs; }
                set { Restrict(); timeoutSecs = value; }
            }

            internal bool inUse = false;
            internal void Restrict()
            {
                if (inUse)
                {
                    throw new MemberAccessException("MccDaq.InputParams are in use and cannot be changed.");
                }
            }
        }

        // Error handling
        private static string _latestError = "No error";

        // Internal buffer and read count
        private static Dictionary<InputParams, float[]> _inputParamsToBuffer = new Dictionary<InputParams, float[]>();
        private static Dictionary<InputParams, int> _inputParamsToNumReadings = new Dictionary<InputParams, int>();

        // Initialize the MCC DAQ
        public static bool Initialize(InputParams p)
        {
            if (_inputParamsToBuffer.ContainsKey(p))
            {
                _latestError = "Inputs already created for the specified parameters.";
                return false;
            }

            // Set up the buffer for temperature readings
            _inputParamsToBuffer[p] = new float[p.BufferSize];
            _inputParamsToNumReadings[p] = 0;

            // Declare the Universal Library version
            float revisionNumber = 5.0f; // Example revision
            int errorCode = cbDeclareRevision(ref revisionNumber);
            if (errorCode != (int)ErrorCode.NoErrors)
            {
                _latestError = "Failed to declare revision.";
                return false;
            }

            // Set error handling to print errors and stop on error
            errorCode = cbErrHandling(1, 1); // Print errors and stop on error
            if (errorCode != (int)ErrorCode.NoErrors)
            {
                _latestError = "Failed to set error handling.";
                return false;
            }

            // Mark the InputParams instance as in use
            p.inUse = true;

            return true;
        }

        // Read the temperature from the thermocouple and store it in the buffer
        public static bool ReadFromInputs(InputParams p)
        {
            if (!_inputParamsToBuffer.ContainsKey(p))
            {
                _latestError = "Cannot read before inputs are created.";
                return false;
            }

            float temperature = 0.0f;

            // Read temperature using cbTIn
            int options = (int)p.Thermocouple;
            int errorCode = cbTIn(p.BoardNum, p.Channel, 0, ref temperature, options); // Scale = 0 for Celsius

            if (errorCode != (int)ErrorCode.NoErrors)
            {
                _latestError = $"Error reading temperature. Code: {errorCode}";
                return false;
            }

            // Store the temperature in the buffer
            int numReadings = _inputParamsToNumReadings[p];
            float[] buffer = _inputParamsToBuffer[p];

            if (numReadings < p.BufferSize)
            {
                buffer[numReadings] = temperature;
                _inputParamsToNumReadings[p]++;
            }
            else
            {
                // If the buffer is full, shift the data and add the new reading
                Array.Copy(buffer, 1, buffer, 0, p.BufferSize - 1);
                buffer[p.BufferSize - 1] = temperature;
            }

            return true;
        }

        // Retrieve the buffer with temperature readings
        public static float[] GetTemperatureBuffer(InputParams p)
        {
            return _inputParamsToBuffer.ContainsKey(p) ? _inputParamsToBuffer[p] : null;
        }

        // Retrieve the number of readings currently in the buffer
        public static int GetNumReadings(InputParams p)
        {
            return _inputParamsToNumReadings.ContainsKey(p) ? _inputParamsToNumReadings[p] : 0;
        }

        // Get the latest error message
        public static string GetLatestError()
        {
            return _latestError;
        }
    }
}

