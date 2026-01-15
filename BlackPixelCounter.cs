using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// Attach to the main camera, to allow the keyboard toggle (Enter key) to 
// switch to a black background when computing the background fraction
// and back to the original background otherwise.
public class BlackPixelCounter : MonoBehaviour
{
    public bool compute = true;
    public bool debug = false;

    public static float BlackPixelPercentage()
    {
        return _blackPercent;
    }

    public void Start()
    {
        _computeShader = (ComputeShader)Resources.Load("BlackPixelCounter");
        if (_computeShader != null)
        {
            _kernelIndex = _computeShader.FindKernel("BlackPixelCounter");

            _renderTextureCaptured = new RenderTexture(Screen.width, Screen.height, 0);

            _computeBuffer = new ComputeBuffer(1, 4);
            _computeBufferResult = new int[1];

            FindCameras(transform);
            if (compute)
            {
                for (int i = 0; i < _cameras.Count; ++i)
                {
                    _cameras[i].camera.clearFlags = CameraClearFlags.SolidColor;
                    _cameras[i].camera.backgroundColor = Color.black;
                }
            }

            _keepCapturing = true;
            StartCoroutine(CaptureFrames());
        }
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (compute)
            {
                compute = false;
                for (int i = 0; i < _cameras.Count; ++i)
                {
                    _cameras[i].camera.clearFlags = _cameras[i].clearFlags;
                    _cameras[i].camera.backgroundColor = _cameras[i].backgroundColor;
                }
            }
            else
            {
                compute = true;
                for (int i = 0; i < _cameras.Count; ++i)
                {
                    _cameras[i].camera.clearFlags = CameraClearFlags.SolidColor;
                    _cameras[i].camera.backgroundColor = Color.black;
                }
            }
        }
    }

    public void OnDisable()
    {
        _keepCapturing = false;
    }

    private IEnumerator CaptureFrames()
    {
        while (_keepCapturing)
        {
            if (debug)
                Debug.Log("CaptureFrames");

            // This note about `CaptureScreenshotAsTexture` applies also to
            // `CaptureScreenshotIntoRenderTexture`:
            // https://docs.unity3d.com/ScriptReference/ScreenCapture.CaptureScreenshotAsTexture.html
            // "To get a reliable output from this method you must make sure it is called once the
            // frame rendering has ended, and not during the rendering process. A simple way of ensuring
            // this is to call it from a coroutine that yields on WaitForEndOfFrame. If you call this
            // method during the rendering process you will get unpredictable and undefined results."
            // With this approach, the image in the render texture seems ready for processing by the
            // compute shader with no additional synchronization.
            yield return new WaitForEndOfFrame();

            if (compute && (_renderTextureCaptured != null) && (_renderTextureCaptured != null))
            {
                ScreenCapture.CaptureScreenshotIntoRenderTexture(_renderTextureCaptured);
                _computeShader.SetTexture(_kernelIndex, "Image", _renderTextureCaptured);

                // The compute shader returns its result---a single integer, with the black pixel count---as
                // the one and only item in the array associated with this ComputeBuffer.
                _computeBufferResult[0] = 0;
                _computeBuffer.SetData(_computeBufferResult);
                _computeShader.SetBuffer(_kernelIndex, "intBuffer", _computeBuffer);

                // Matches `[numthreads(32, 32, 1)]` in the shader.
                int threadGroupsX = _renderTextureCaptured.width / 32;
                int threadGroupsY = _renderTextureCaptured.height / 32;
                _computeShader.Dispatch(_kernelIndex, threadGroupsX, threadGroupsY, 1);

                AsyncGPUReadback.Request(_computeBuffer, ComputeReadbackCompleted);
            }
            else
            {
                _blackPercent = -1;
            }
        }
    }

    private void ComputeReadbackCompleted(AsyncGPUReadbackRequest request)
    {
        if (_renderTextureCaptured != null)
        {
            if (debug)
                Debug.Log("ComputeReadbackCompleted");

            // Get the result from the compute shader.
            _computeBufferResult = request.GetData<int>().ToArray();
            int blackPixelCount = _computeBufferResult[0];

            int allPixelCount = _renderTextureCaptured.width * _renderTextureCaptured.height;
            float blackFrac = ((float)blackPixelCount) / allPixelCount;
            _blackPercent = 100 * blackFrac;

            if (debug)
                Debug.Log("Black pixel count " + blackPixelCount + " of " + allPixelCount + " or " + 
                    Math.Round(_blackPercent, 1) + "%");
        }
    }

    private void FindCameras(Transform current)
    {
        Camera camera = current.gameObject.GetComponent<Camera>();
        if (camera != null)
        {
            _cameras.Add(new CameraData(camera));
        }
        for (int iChild = 0; iChild < current.childCount; ++iChild)
        {
            FindCameras(current.GetChild(iChild));
        }
    }

    private ComputeShader _computeShader;
    private int _kernelIndex;

    private struct CameraData
    {
        public Camera camera;
        public CameraClearFlags clearFlags;
        public Color backgroundColor;
        public CameraData(Camera c)
        {
            camera = c;
            clearFlags = c.clearFlags;
            backgroundColor = c.backgroundColor;
        }
    }
    private List<CameraData> _cameras = new List<CameraData>();

    private bool _keepCapturing;
    private RenderTexture _renderTextureCaptured;

    private ComputeBuffer _computeBuffer;
    private int[] _computeBufferResult;

    private static float _blackPercent = -1;
}
