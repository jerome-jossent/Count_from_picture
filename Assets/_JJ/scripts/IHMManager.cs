using OpenCVForUnity.UnityUtils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IHMManager : MonoBehaviour
{
    ScriptsManager _sm;

    private void Awake()
    {
        _sm = GameObject.Find("ScriptsManager").GetComponent<ScriptsManager>();
    }
    Color32[] colors; 
    Texture2D texture;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       // Utils.matToTexture2D(_sm._sourceManager._source_Mat, texture, colors);

    }



    void Dispose()
    {

        if (texture != null)
        {
            Texture2D.Destroy(texture);
            texture = null;
        }
    }

    //void OnInited()
    //{
    //    if (colors == null || colors.Length != webCamTexture.width * webCamTexture.height)
    //        colors = new Color32[webCamTexture.width * webCamTexture.height];
    //    if (texture == null || texture.width != webCamTexture.width || texture.height != webCamTexture.height)
    //        texture = new Texture2D(webCamTexture.width, webCamTexture.height, TextureFormat.RGBA32, false);

    //    _WebCam_Mat = new Mat(webCamTexture.height, webCamTexture.width, CvType.CV_8UC4, new Scalar(0, 0, 0, 255));
    //    Utils.matToTexture2D(_WebCam_Mat, texture, colors);

    //    gameObject.GetComponent<Renderer>().material.mainTexture = texture;

    //    gameObject.transform.localScale = new Vector3(webCamTexture.width, webCamTexture.height, 1);
    //    Debug.Log("Screen.width " + Screen.width + " Screen.height " + Screen.height + " Screen.orientation " + Screen.orientation);

    //    if (fpsMonitor != null)
    //    {
    //        fpsMonitor.Add("width", _WebCam_Mat.width().ToString());
    //        fpsMonitor.Add("height", _WebCam_Mat.height().ToString());
    //        fpsMonitor.Add("orientation", Screen.orientation.ToString());
    //    }

    //    float width = _WebCam_Mat.width();
    //    float height = _WebCam_Mat.height();

    //    float widthScale = (float)Screen.width / width;
    //    float heightScale = (float)Screen.height / height;
    //    if (widthScale < heightScale)
    //        Camera.main.orthographicSize = (width * (float)Screen.height / (float)Screen.width) / 2;
    //    else
    //        Camera.main.orthographicSize = height / 2;
    //}
}
