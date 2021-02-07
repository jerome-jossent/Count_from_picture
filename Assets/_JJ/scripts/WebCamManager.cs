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
    [SerializeField] Parametre threshold_thresh_valeur;

    enum Output_type { original, gray, binary, augmented }
    [SerializeField] Output_type output_Type;
    public TMPro.TMP_Dropdown output_Type_dropdown;

    PictureToCount.Seuillage_type seuillage_Type;
    public TMPro.TMP_Dropdown seuillage_Type_dropdown;

    public TMPro.TMP_Text count_text;
    public bool stream_on;

    private void Awake()
    {
        threshold_thresh_valeur._SetValue(50);


        output_Type_dropdown.options.Clear();
        foreach (var item in Enum.GetValues(typeof(Output_type)))
            output_Type_dropdown.options.Add(new TMPro.TMP_Dropdown.OptionData(item.ToString()));

        output_Type_dropdown.value = output_Type_dropdown.options.Count - 1;
        _output_Type_dropdown_Change();


        seuillage_Type_dropdown.options.Clear();
        foreach (var item in Enum.GetValues(typeof(PictureToCount.Seuillage_type)))
            seuillage_Type_dropdown.options.Add(new TMPro.TMP_Dropdown.OptionData(item.ToString()));

        seuillage_Type_dropdown.value = 0;
        _seuillage_Type_dropdown_Change();

        stream_on = true;
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
    Mat rgbaMat;
    Color32[] colors;
    Texture2D texture;
    bool isInitWaiting = false;
    bool hasInitDone = false;
    FpsMonitor fpsMonitor;
    #endregion

    #region WebCam

    void Start()
    {
        fpsMonitor = GetComponent<FpsMonitor>();

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
            if (fpsMonitor != null)
            {
                fpsMonitor.consoleText = "Camera permission is denied.";
            }
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
        if (rgbaMat != null)
        {
            rgbaMat.Dispose();
            rgbaMat = null;
        }
        if (texture != null)
        {
            Texture2D.Destroy(texture);
            texture = null;
        }
    }

    // Raises the webcam texture initialized event.
    void OnInited()
    {
        if (colors == null || colors.Length != webCamTexture.width * webCamTexture.height)
            colors = new Color32[webCamTexture.width * webCamTexture.height];
        if (texture == null || texture.width != webCamTexture.width || texture.height != webCamTexture.height)
            texture = new Texture2D(webCamTexture.width, webCamTexture.height, TextureFormat.RGBA32, false);

        rgbaMat = new Mat(webCamTexture.height, webCamTexture.width, CvType.CV_8UC4, new Scalar(0, 0, 0, 255));
        Utils.matToTexture2D(rgbaMat, texture, colors);

        gameObject.GetComponent<Renderer>().material.mainTexture = texture;

        gameObject.transform.localScale = new Vector3(webCamTexture.width, webCamTexture.height, 1);
        Debug.Log("Screen.width " + Screen.width + " Screen.height " + Screen.height + " Screen.orientation " + Screen.orientation);

        if (fpsMonitor != null)
        {
            fpsMonitor.Add("width", rgbaMat.width().ToString());
            fpsMonitor.Add("height", rgbaMat.height().ToString());
            fpsMonitor.Add("orientation", Screen.orientation.ToString());
        }

        float width = rgbaMat.width();
        float height = rgbaMat.height();

        float widthScale = (float)Screen.width / width;
        float heightScale = (float)Screen.height / height;
        if (widthScale < heightScale)
            Camera.main.orthographicSize = (width * (float)Screen.height / (float)Screen.width) / 2;
        else
            Camera.main.orthographicSize = height / 2;
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
        if (hasInitDone && webCamTexture.isPlaying && webCamTexture.didUpdateThisFrame)
        {
            if (stream_on)
                Utils.webCamTextureToMat(webCamTexture, rgbaMat, colors);

            PictureToCount.Compute(rgbaMat, seuillage_Type, threshold_thresh_valeur._valeur);

            Mat HMI = null;
            switch (output_Type)
            {
                case Output_type.original:
                    HMI = PictureToCount.original;
                    break;
                case Output_type.gray:
                    HMI = new Mat();
                    Imgproc.cvtColor(PictureToCount.gray, HMI, Imgproc.COLOR_GRAY2RGB);
                    break;
                case Output_type.binary:
                    HMI = new Mat();
                    Imgproc.cvtColor(PictureToCount.bw, HMI, Imgproc.COLOR_GRAY2RGB);
                    break;
                case Output_type.augmented:
                    HMI = PictureToCount.augmented;
                    break;
            }

            //string message = PictureToCount.contours_filtered_count + " / " + PictureToCount.contours.Count;
            string message = PictureToCount.contours_filtered_count.ToString();

            //Imgproc.putText(HMI, message,
            //    new Point(5, rgbaMat.rows() - 10), Imgproc.FONT_HERSHEY_SIMPLEX,
            //    1.0, new Scalar(255, 0, 0, 0), 2, Imgproc.LINE_AA, false);

            count_text.text = message;

            Utils.matToTexture2D(HMI, texture, colors);
        }
    }

    public void _output_Type_dropdown_Change()
    {
        TMPro.TMP_Dropdown.OptionData od = output_Type_dropdown.options[output_Type_dropdown.value];
        Enum.TryParse(od.text, out output_Type);
    }
    public void _seuillage_Type_dropdown_Change()
    {
        TMPro.TMP_Dropdown.OptionData od = seuillage_Type_dropdown.options[seuillage_Type_dropdown.value];
        Enum.TryParse(od.text, out seuillage_Type);
    }

    public void _stream_on_switch()
    {
        stream_on = !stream_on;
    }

}
