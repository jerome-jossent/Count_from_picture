using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnityExample;
using System;

public class WebCamManager : MonoBehaviour
{
    public bool _initialized;
    public bool _stream_on;
    public Mat _webCam_Mat;
    ScriptsManager _sm;

    private void Awake()
    {
        _sm = GameObject.Find("ScriptsManager").GetComponent<ScriptsManager>();
        _initialized = false;
        _stream_on = false;
    }

    #region VARIABLES
    [SerializeField, TooltipAttribute("Set the name of the device to use.")]
    public string requestedDeviceName = null;
    [SerializeField, TooltipAttribute("Set the width of WebCamTexture.")]
    public int requestedWidth = 640;
    [SerializeField, TooltipAttribute("Set the height of WebCamTexture.")]
    public int requestedHeight = 480;
    [SerializeField, TooltipAttribute("Set FPS of WebCamTexture.")]
    public int requestedFPS = 30;
    [SerializeField, TooltipAttribute("Set whether to use the front facing camera.")]
    public bool requestedIsFrontFacing = false;

    WebCamTexture webCamTexture;
    WebCamDevice webCamDevice;
    //Color32[] colors;
    //Texture2D texture;
    bool isInitWaiting = false;
    bool hasInitDone = false;
    //FpsMonitor fpsMonitor;
    #endregion

    #region WebCam

    void Start()
    {
        //fpsMonitor = GetComponent<FpsMonitor>();

        WebCamTexture_Initialize();
    }

    void WebCamTexture_Initialize()
    {
        if (isInitWaiting)
            return;

#if UNITY_ANDROID && !UNITY_EDITOR
            // Set the requestedFPS parameter to avoid the problem of the WebCamTexture image becoming low light on some Android devices (e.g. Google Pixel, Pixel2).
            // https://forum.unity.com/threads/android-webcamtexture-in-low-light-only-some-models.520656/
            // https://forum.unity.com/threads/released-opencv-for-unity.277080/page-33#post-3445178
            if (requestedIsFrontFacing)
            {
                int rearCameraFPS = requestedFPS;
                requestedFPS = 15;
                StartCoroutine(WebCamTexture_Initialize_coroutine());
                requestedFPS = rearCameraFPS;
            }
            else
            {
                StartCoroutine(WebCamTexture_Initialize_coroutine());
            }
#else
        StartCoroutine(WebCamTexture_Initialize_coroutine());
#endif
    }

    IEnumerator WebCamTexture_Initialize_coroutine()
    {
        if (hasInitDone)
            Dispose();

        isInitWaiting = true;

        // Checks camera permission state.
#if UNITY_IOS && UNITY_2018_1_OR_NEWER
            UserAuthorization mode = UserAuthorization.WebCam;
            if (!Application.HasUserAuthorization(mode))
            {
                isUserRequestingPermission = true;
                yield return Application.RequestUserAuthorization(mode);

                float timeElapsed = 0;
                while (isUserRequestingPermission)
                {
                    if (timeElapsed > 0.25f)
                    {
                        isUserRequestingPermission = false;
                        break;
                    }
                    timeElapsed += Time.deltaTime;

                    yield return null;
                }
            }

            if (!Application.HasUserAuthorization(mode))
            {
                if (fpsMonitor != null)
                {
                    fpsMonitor.consoleText = "Camera permission is denied.";
                }
                isInitWaiting = false;
                yield break;
            }
#elif UNITY_ANDROID && UNITY_2018_3_OR_NEWER
        string permission = UnityEngine.Android.Permission.Camera;
        if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(permission))
        {
            isUserRequestingPermission = true;
            UnityEngine.Android.Permission.RequestUserPermission(permission);

            float timeElapsed = 0;
            while (isUserRequestingPermission)
            {
                if (timeElapsed > 0.25f)
                {
                    isUserRequestingPermission = false;
                    break;
                }
                timeElapsed += Time.deltaTime;

                yield return null;
            }
        }

        if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(permission))
        {
            //if (fpsMonitor != null)
            //{
            //    fpsMonitor.consoleText = "Camera permission is denied.";
            //}
            isInitWaiting = false;
            yield break;
        }
#endif

        // Creates the camera
        var devices = WebCamTexture.devices;
        if (!String.IsNullOrEmpty(requestedDeviceName))
        {
            int requestedDeviceIndex = -1;
            if (Int32.TryParse(requestedDeviceName, out requestedDeviceIndex))
            {
                if (requestedDeviceIndex >= 0 && requestedDeviceIndex < devices.Length)
                {
                    webCamDevice = devices[requestedDeviceIndex];
                    webCamTexture = new WebCamTexture(webCamDevice.name, requestedWidth, requestedHeight, requestedFPS);
                }
            }
            else
            {
                for (int cameraIndex = 0; cameraIndex < devices.Length; cameraIndex++)
                {
                    if (devices[cameraIndex].name == requestedDeviceName)
                    {
                        webCamDevice = devices[cameraIndex];
                        webCamTexture = new WebCamTexture(webCamDevice.name, requestedWidth, requestedHeight, requestedFPS);
                        break;
                    }
                }
            }
            if (webCamTexture == null)
                Debug.Log("Cannot find camera device " + requestedDeviceName + ".");
        }

        if (webCamTexture == null)
        {
            // Checks how many and which cameras are available on the device
            for (int cameraIndex = 0; cameraIndex < devices.Length; cameraIndex++)
            {
#if UNITY_2018_3_OR_NEWER
                if (devices[cameraIndex].kind != WebCamKind.ColorAndDepth && devices[cameraIndex].isFrontFacing == requestedIsFrontFacing)
#else
                    if (devices[cameraIndex].isFrontFacing == requestedIsFrontFacing)
#endif
                {
                    webCamDevice = devices[cameraIndex];
                    webCamTexture = new WebCamTexture(webCamDevice.name, requestedWidth, requestedHeight, requestedFPS);
                    break;
                }
            }
        }

        if (webCamTexture == null)
        {
            if (devices.Length > 0)
            {
                webCamDevice = devices[0];
                webCamTexture = new WebCamTexture(webCamDevice.name, requestedWidth, requestedHeight, requestedFPS);
            }
            else
            {
                Debug.LogError("Camera device does not exist.");
                isInitWaiting = false;
                yield break;
            }
        }

        // Starts the camera.
        webCamTexture.Play();

        while (true)
        {
            if (webCamTexture.didUpdateThisFrame)
            {
                Debug.Log("name:" + webCamTexture.deviceName + " width:" + webCamTexture.width + " height:" + webCamTexture.height + " fps:" + webCamTexture.requestedFPS);
                Debug.Log("videoRotationAngle:" + webCamTexture.videoRotationAngle + " videoVerticallyMirrored:" + webCamTexture.videoVerticallyMirrored + " isFrongFacing:" + webCamDevice.isFrontFacing);

                isInitWaiting = false;
                hasInitDone = true;

                OnInited();

                break;
            }
            else
            {
                yield return null;
            }
        }
    }

#if (UNITY_IOS && UNITY_2018_1_OR_NEWER) || (UNITY_ANDROID && UNITY_2018_3_OR_NEWER)
    bool isUserRequestingPermission;

    IEnumerator OnApplicationFocus(bool hasFocus)
    {
        yield return null;

        if (isUserRequestingPermission && hasFocus)
            isUserRequestingPermission = false;
    }
#endif

    void Dispose()
    {
        isInitWaiting = false;
        hasInitDone = false;

        if (webCamTexture != null)
        {
            webCamTexture.Stop();
            WebCamTexture.Destroy(webCamTexture);
            webCamTexture = null;
        }
        if (_webCam_Mat != null)
        {
            _webCam_Mat.Dispose();
            _webCam_Mat = null;
        }
        //if (texture != null)
        //{
        //    Texture2D.Destroy(texture);
        //    texture = null;
        //}
    }

    // Raises the webcam texture initialized event.
    void OnInited()
    {
        _stream_on = true;

        //if (colors == null || colors.Length != webCamTexture.width * webCamTexture.height)
        //    colors = new Color32[webCamTexture.width * webCamTexture.height];
        //if (texture == null || texture.width != webCamTexture.width || texture.height != webCamTexture.height)
        //    texture = new Texture2D(webCamTexture.width, webCamTexture.height, TextureFormat.RGBA32, false);

        //_WebCam_Mat = new Mat(webCamTexture.height, webCamTexture.width, CvType.CV_8UC4, new Scalar(0, 0, 0, 255));
        //Utils.matToTexture2D(_WebCam_Mat, texture, colors);

        //gameObject.GetComponent<Renderer>().material.mainTexture = texture;

        //gameObject.transform.localScale = new Vector3(webCamTexture.width, webCamTexture.height, 1);
        //Debug.Log("Screen.width " + Screen.width + " Screen.height " + Screen.height + " Screen.orientation " + Screen.orientation);

        //if (fpsMonitor != null)
        //{
        //    fpsMonitor.Add("width", _WebCam_Mat.width().ToString());
        //    fpsMonitor.Add("height", _WebCam_Mat.height().ToString());
        //    fpsMonitor.Add("orientation", Screen.orientation.ToString());
        //}

        //float width = _WebCam_Mat.width();
        //float height = _WebCam_Mat.height();

        //float widthScale = (float)Screen.width / width;
        //float heightScale = (float)Screen.height / height;
        //if (widthScale < heightScale)
        //    Camera.main.orthographicSize = (width * (float)Screen.height / (float)Screen.width) / 2;
        //else
        //    Camera.main.orthographicSize = height / 2;
    }

    void OnDestroy()
    {
        Dispose();
    }

    // Raises the play button click event.
    public void OnPlayButtonClick()
    {
        if (hasInitDone)
            webCamTexture.Play();
    }

    // Raises the pause button click event.
    public void OnPauseButtonClick()
    {
        if (hasInitDone)
            webCamTexture.Pause();
    }

    // Raises the stop button click event.
    public void OnStopButtonClick()
    {
        if (hasInitDone)
            webCamTexture.Stop();
    }

    // Raises the change camera button click event.
    public void OnChangeCameraButtonClick()
    {
        if (hasInitDone)
        {
            requestedDeviceName = null;
            requestedIsFrontFacing = !requestedIsFrontFacing;
            WebCamTexture_Initialize();
        }
    }
    #endregion

    void Update()
    {
        //if (hasInitDone && webCamTexture.isPlaying && webCamTexture.didUpdateThisFrame)
        //{
        //    if (_stream_on)
        //        Utils.webCamTextureToMat(webCamTexture, _webCam_Mat, _sm. colors);
        //}
    }
}
