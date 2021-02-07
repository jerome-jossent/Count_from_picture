using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ImageProcess : MonoBehaviour
{
    ScriptsManager _sm;

    private void Awake()
    {
        _sm = GameObject.Find("ScriptsManager").GetComponent<ScriptsManager>();
    }

    void Start()
    {
        _sm._webcam_Manager._OnNewPicture.AddListener(ComputePicture);
    }

    void ComputePicture()
    {
        //image process
        PictureToCount.Compute(_sm._webcam_Manager._webcam_rgbaMat, PictureToCount.Seuillage_type.manuel, 50);

        //display image
        _sm._webcam_Manager._SetPicture(_sm._sourceManager._GetAskedPicture());
    }
}
