// A simple example of how to use `Janelia.KinematicSubject`.
// In this case, the `IKinematicUpdater` instance checks the keyboard arrow keys
// to let the user specify the translation and rotation.

using UnityEngine;

//for socket communication
using System;
using System.IO;
using System.Net.Sockets;

namespace Janelia
{
    public class flyUpdate : Janelia.KinematicSubject
    {
        // Note: Game default assumes that 1 game unit = 1 m. Units affect gravity.
        // Thus, try to keep objects to real-world scale.
        public float ballRad = 0.5f;
        public string localAddress = "10.102.40.125";
        public int port = 2000;

        public class FictracUpdater : Janelia.KinematicSubject.IKinematicUpdater
        {
            public float deltaRotation = 1.0f;
            public float deltaTranslationX = 1.0f;
            public float deltaTranslationY = 1.0f;
            bool positionUpdate = false;

            // fields related to fictract value conversion (set defaults, but update in constructor)
            float ballR = 0.5f;

            // fields related to socket connection (set defaults, but update in constructor)
            string hostAddress = "10.102.40.125";
            int portNum = 2000;
            private TcpClient mySocket;
            internal Boolean socketReady = false;
            private NetworkStream theStream;
            private StreamReader theReader;

            // fields related to fictrac info
            private string ftdataraw = "";
            private float[] ftdelta = new float[3]; //will hold delta rotation vector [fictrac data col 6-8]
            private float[] ftpos = new float[3]; //will hold current x, y and heading pos [fictrac data col 15, 16, 17]

            // Use constructor to set some fileds
            public FictracUpdater(string localAddress, int port, float ballRad)
            {
                hostAddress = localAddress;
                portNum = port;
                ballR = ballRad;
            }

            // When the application starts, start a connection to the specified socket port
            public void Start()
            {
                // server (fictrac) needs to have connected to port already
                // if no client has connected yet, fictrac will wait to start up
                Debug.Log("Setting up socket connection");
                try
                {
                    mySocket = new TcpClient(hostAddress, portNum);
                    //Note: There is an alterative, TcpListener, which may be more appropriate..?
                    theStream = mySocket.GetStream();
                    theReader = new StreamReader(theStream);
                    socketReady = true;
                    Debug.Log("Socket connection open");
                }
                catch (Exception e)
                {
                    Debug.Log("Socket error: " + e);
                }

            }


            public void Update()
            {
                // On each frame, check for new data from socket
                if (socketReady & theStream.DataAvailable)
                {
                    ftdataraw = theReader.ReadLine();

                    // compute position updates based on fictrac input
                    // parse fictrac data to get just x and y pos
                    var ftdata = ftdataraw.Split(',');
                    if (ftdata.Length < 17)
                    {
                        Debug.Log("No fictrac data");
                        positionUpdate = false;
                    }
                    else
                    {
                        var scalingFactor = (float)Math.PI / 180.0f;
                        ftdelta[0] = (float)Convert.ToDouble(ftdata[6]) * ballR;
                        ftdelta[1] = (float)Convert.ToDouble(ftdata[7]) * ballR;
                        ftdelta[2] = (float)Convert.ToDouble(ftdata[8]) * scalingFactor;

                        deltaTranslationX = ftdelta[0];
                        deltaTranslationY = ftdelta[1];
                        deltaRotation = ftdelta[2];

                        positionUpdate = true;

                    }
                }

            }

            //Position updates:
            //Vector3:  x -> forward/backward movement (pitch)
            //          y -> rotation (yaw)
            //          z -> sideways motion (roll)

            // Must be defined in this subclass.
            public Vector3? Translation()
            {
                if (Input.GetKey("q") || Input.GetKey(KeyCode.Escape))
                {
                    Application.Quit();
                }

                // position updates based on fictrac
                if (positionUpdate)
                {
                    positionUpdate = false;
                    return new Vector3(deltaTranslationX, 0, deltaTranslationY);
                }
                
                return null;
            }

            // Must be defined in this subclass.
            public Vector3? RotationDegrees()
            {
                // replace the parts below with computed position updates based on fictrac
                if (positionUpdate)
                {
                    positionUpdate = false;
                    return new Vector3(0, deltaRotation, 0);
                }

                return null;
            }
        }

        new void Start()
        {
            // The `updater` field (inherited from the `KinematicSubject` base class)
            // must be set in this `Start()` method.
            updater = new FictracUpdater(localAddress, port, ballRad);

            // The `collisionRadius` and `collisionPlaneNormal` fields are optional,
            // and if set, they are passed to the `Janelia.KinematicCollisionHandler`
            // created in the base class.
            collisionRadius = 1.0f;
            foreach (Transform child in transform)
            {
                if (child.gameObject.name.EndsWith("Marker"))
                {
                    collisionRadius = Mathf.Max(child.localScale.x, child.localScale.z);
                }
            }
            collisionPlaneNormal = new Vector3(0, 1, 0);

            // Let the base class finish the initial set-up.
            base.Start();
        }
    }
}