#if !(PLATFORM_LUMIN && !UNITY_EDITOR)

using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.UnityUtils.Helper;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace OpenCVForUnityExample
{
    //JJ edition
    [RequireComponent(typeof(WebCamTextureToMatHelper_JJO))]
    public class Webcam_Manager : MonoBehaviour
    {
        public UnityEvent _OnNewPicture = new UnityEvent();

        public Dropdown requestedResolutionDropdown;
        public ResolutionPreset requestedResolution = ResolutionPreset._640x480;
        public FPSPreset requestedFPS = FPSPreset._30;
        public Toggle flipVerticalToggle;
        public Toggle flipHorizontalToggle;
        public Mat _webcam_rgbaMat;

        ScriptsManager _sm;
        Texture2D texture;
        Material material;
        WebCamTextureToMatHelper_JJO webCamTextureToMatHelper;
        //FpsMonitor fpsMonitor;

        private void Awake()
        {
            _sm = GameObject.Find("ScriptsManager").GetComponent<ScriptsManager>();
        }

        void Start()
        {
            material = gameObject.GetComponent<Renderer>().material;
            //fpsMonitor = GetComponent<FpsMonitor>();

            webCamTextureToMatHelper = gameObject.GetComponent<WebCamTextureToMatHelper_JJO>();
            int width, height;
            Dimensions(requestedResolution, out width, out height);
            webCamTextureToMatHelper.requestedWidth = width;
            webCamTextureToMatHelper.requestedHeight = height;
            webCamTextureToMatHelper.requestedFPS = (int)requestedFPS;
            webCamTextureToMatHelper.Initialize();

            //supression 9999x9999
            requestedResolutionDropdown.options.RemoveAt(requestedResolutionDropdown.options.Count-1);

            //// Update GUI state
            //requestedResolutionDropdown.value = (int)requestedResolution;
            //string[] enumNames = System.Enum.GetNames(typeof(FPSPreset));
            //int index = Array.IndexOf(enumNames, requestedFPS.ToString());

            flipVerticalToggle.isOn = webCamTextureToMatHelper.flipVertical;
            flipHorizontalToggle.isOn = webCamTextureToMatHelper.flipHorizontal;
        }

        public void OnWebCamTextureToMatHelperInitialized()
        {
            Debug.Log("OnWebCamTextureToMatHelperInitialized");

            Mat webCamTextureMat = webCamTextureToMatHelper.GetMat();

            texture = new Texture2D(webCamTextureMat.cols(), webCamTextureMat.rows(), TextureFormat.RGBA32, false);
            Utils.fastMatToTexture2D(webCamTextureMat, texture);

            gameObject.GetComponent<Renderer>().material.mainTexture = texture;

            gameObject.transform.localScale = new Vector3(webCamTextureMat.cols(), webCamTextureMat.rows(), 1);
            Debug.Log("Screen.width " + Screen.width + " Screen.height " + Screen.height + " Screen.orientation " + Screen.orientation);

            //if (fpsMonitor != null)
            //{
            //    fpsMonitor.Add("deviceName", webCamTextureToMatHelper.GetDeviceName().ToString());
            //    fpsMonitor.Add("width", webCamTextureToMatHelper.GetWidth().ToString());
            //    fpsMonitor.Add("height", webCamTextureToMatHelper.GetHeight().ToString());
            //    fpsMonitor.Add("videoRotationAngle", webCamTextureToMatHelper.GetWebCamTexture().videoRotationAngle.ToString());
            //    fpsMonitor.Add("videoVerticallyMirrored", webCamTextureToMatHelper.GetWebCamTexture().videoVerticallyMirrored.ToString());
            //    fpsMonitor.Add("camera fps", webCamTextureToMatHelper.GetFPS().ToString());
            //    fpsMonitor.Add("isFrontFacing", webCamTextureToMatHelper.IsFrontFacing().ToString());
            //    fpsMonitor.Add("rotate90Degree", webCamTextureToMatHelper.rotate90Degree.ToString());
            //    fpsMonitor.Add("flipVertical", webCamTextureToMatHelper.flipVertical.ToString());
            //    fpsMonitor.Add("flipHorizontal", webCamTextureToMatHelper.flipHorizontal.ToString());
            //    fpsMonitor.Add("orientation", Screen.orientation.ToString());
            //}


            float width = webCamTextureMat.width();
            float height = webCamTextureMat.height();

            float widthScale = (float)Screen.width / width;
            float heightScale = (float)Screen.height / height;
            if (widthScale < heightScale)
            {
                Camera.main.orthographicSize = (width * (float)Screen.height / (float)Screen.width) / 2;
            }
            else
            {
                Camera.main.orthographicSize = height / 2;
            }
        }

        public void OnWebCamTextureToMatHelperDisposed()
        {
            Debug.Log("OnWebCamTextureToMatHelperDisposed");

            if (texture != null)
            {
                Texture2D.Destroy(texture);
                texture = null;
            }
        }

        public void OnWebCamTextureToMatHelperErrorOccurred(WebCamTextureToMatHelper.ErrorCode errorCode)
        {
            Debug.Log("OnWebCamTextureToMatHelperErrorOccurred " + errorCode);

            //if (fpsMonitor != null)
            //{
            //    fpsMonitor.consoleText = "ErrorCode: " + errorCode;
            //}
        }

        void Update()
        {
            if (webCamTextureToMatHelper.IsPlaying() && webCamTextureToMatHelper.DidUpdateThisFrame())
            {
                _webcam_rgbaMat = webCamTextureToMatHelper.GetMat();

                _OnNewPicture.Invoke();
                //Imgproc.putText (rgbaMat, "W:" + rgbaMat.width () + " H:" + rgbaMat.height () + " SO:" + Screen.orientation, new Point (5, rgbaMat.rows () - 10), Imgproc.FONT_HERSHEY_SIMPLEX, 1.0, new Scalar (255, 255, 255, 255), 2, Imgproc.LINE_AA, false);

                //Utils.fastMatToTexture2D(_webcam_rgbaMat, texture);
            }
        }

        public void _SetPicture(Mat matToDisplay)
        {
            if (texture == null)
                texture = new Texture2D(matToDisplay.cols(), matToDisplay.rows(), TextureFormat.RGBA32, false);

            Utils.fastMatToTexture2D(matToDisplay, texture);

            material.mainTexture = texture;
        }

        void OnDestroy()
        {
            webCamTextureToMatHelper.Dispose();
        }

        public void OnBackButtonClick()
        {
            SceneManager.LoadScene("OpenCVForUnityExample");
        }

        public void OnPlayButtonClick()
        {
            webCamTextureToMatHelper.Play();
        }

        public void OnPauseButtonClick()
        {
            webCamTextureToMatHelper.Pause();
        }

        public void OnStopButtonClick()
        {
            webCamTextureToMatHelper.Stop();
        }

        public void OnChangeCameraButtonClick()
        {
            webCamTextureToMatHelper.requestedIsFrontFacing = !webCamTextureToMatHelper.IsFrontFacing();
        }

        public void OnRequestedResolutionDropdownValueChanged(int result)
        {
            if ((int)requestedResolution != result)
            {
                requestedResolution = (ResolutionPreset)result;

                int width, height;
                Dimensions(requestedResolution, out width, out height);

                webCamTextureToMatHelper.Initialize(width, height);
            }
        }

        public void OnRequestedFPSDropdownValueChanged(int result)
        {
            string[] enumNames = Enum.GetNames(typeof(FPSPreset));
            int value = (int)System.Enum.Parse(typeof(FPSPreset), enumNames[result], true);

            if ((int)requestedFPS != value)
            {
                requestedFPS = (FPSPreset)value;

                webCamTextureToMatHelper.requestedFPS = (int)requestedFPS;
            }
        }

        public void OnRotate90DegreeToggleValueChanged()
        {

        }

        public void OnFlipVerticalToggleValueChanged()
        {
            if (flipVerticalToggle.isOn != webCamTextureToMatHelper.flipVertical)
            {
                webCamTextureToMatHelper.flipVertical = flipVerticalToggle.isOn;
            }

            //if (fpsMonitor != null)
            //    fpsMonitor.Add("flipVertical", webCamTextureToMatHelper.flipVertical.ToString());
        }

        public void OnFlipHorizontalToggleValueChanged()
        {
            if (flipHorizontalToggle.isOn != webCamTextureToMatHelper.flipHorizontal)
            {
                webCamTextureToMatHelper.flipHorizontal = flipHorizontalToggle.isOn;
            }

            //if (fpsMonitor != null)
            //    fpsMonitor.Add("flipHorizontal", webCamTextureToMatHelper.flipHorizontal.ToString());
        }

        public enum FPSPreset : int
        {
            _0 = 0,
            _1 = 1,
            _5 = 5,
            _10 = 10,
            _15 = 15,
            _30 = 30,
            _60 = 60,
        }

        public enum ResolutionPreset : byte
        {
            _50x50 = 0,
            _640x480,
            _1280x720,
            _1920x1080,
            _9999x9999,
        }

        private void Dimensions(ResolutionPreset preset, out int width, out int height)
        {
            switch (preset)
            {
                case ResolutionPreset._50x50:
                    width = 50;
                    height = 50;
                    break;
                case ResolutionPreset._640x480:
                    width = 640;
                    height = 480;
                    break;
                case ResolutionPreset._1280x720:
                    width = 1280;
                    height = 720;
                    break;
                case ResolutionPreset._1920x1080:
                    width = 1920;
                    height = 1080;
                    break;
                case ResolutionPreset._9999x9999:
                    width = 9999;
                    height = 9999;
                    break;
                default:
                    width = height = 0;
                    break;
            }
        }
    }
}
#endif