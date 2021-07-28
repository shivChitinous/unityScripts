using System;
using System.Collections.Generic;
using UnityEngine;

namespace Janelia
{
    public class ExampleUsingFicTrac : MonoBehaviour
    {
        public string ficTracServerAddress = "127.0.0.1";
        public int ficTracServerPort = 2000;

        public void Start()
        {
            _ficTracReader = new FicTracReader(ficTracServerAddress, ficTracServerPort);
            _ficTracReader.Start();
        }

        public void Update()
        {
            Byte[] dataFromSocket = null;
            long dataTimestampMs = 0;
            int i0 = -1;
            while (_ficTracReader.GetNextMessage(ref dataFromSocket, ref dataTimestampMs, ref i0))
            {
                bool valid = true;

                // https://github.com/rjdmoore/fictrac/blob/master/doc/data_header.txt
                // COL     PARAMETER                       DESCRIPTION
                // 1       frame counter                   Corresponding video frame(starts at #1).
                // 6-8     delta rotation vector (lab)     Change in orientation since last frame,
                //                                         represented as rotation angle / axis(radians)
                //                                         in laboratory coordinates(see
                //                                         * configImg.jpg).

                int i6 = 0, len6 = 0;
                FicTracUtilities.NthSplit(dataFromSocket, i0, 6, ref i6, ref len6);
                float a = (float)FicTracUtilities.AtoF(dataFromSocket, i6, len6, ref valid);
                if (!valid)
                    break;

                int i7 = 0, len7 = 0;
                FicTracUtilities.NthSplit(dataFromSocket, i0, 7, ref i7, ref len7);
                float b = (float)FicTracUtilities.AtoF(dataFromSocket, i7, len7, ref valid);
                if (!valid)
                    break;

                int i8 = 0, len8 = 0;
                FicTracUtilities.NthSplit(dataFromSocket, i0, 8, ref i8, ref len8);
                float c = (float)FicTracUtilities.AtoF(dataFromSocket, i8, len8, ref valid);
                if (!valid)
                    break;

                // https://www.researchgate.net/figure/Visual-output-from-the-FicTrac-software-see-supplementary-video-a-A-segment-of-the_fig2_260044337
                // Rotation about `W_x` is forward/backward translation
                // Rotation about `W_y` is sideways translation
                // Rotation about `W_z` is heading change

                float forward = a * Mathf.Rad2Deg;
                float sideways = b * Mathf.Rad2Deg;
                _translation.Set(forward, 0, sideways);

                float heading = c * Mathf.Rad2Deg;
                _rotation.Set(0, heading, 0);

                transform.Translate(_translation);
                transform.Rotate(_rotation);
            }
        }

        public void OnDisable()
        {
            _ficTracReader.OnDisable();
        }

        private FicTracReader _ficTracReader;
        private Vector3 _translation = new Vector3();
        private Vector3 _rotation = new Vector3();
    }
}
