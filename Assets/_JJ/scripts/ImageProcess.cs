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
       // _sm._webcam_Manager._OnNewPicture.AddListener(ComputePicture);
    }

  public  void ComputePicture()
    {
        //image process
        PictureToCount.Compute(_sm._webcam_Manager._webcam_rgbaMat,
                               _sm._sourceManager.seuillage_Type,
                               _sm._sourceManager.threshold_thresh_valeur._valeur);

        _sm._sourceManager.count_text.text = PictureToCount.contours_filtered_count.ToString();

        //display image
        _sm._webcam_Manager._SetPicture(_sm._sourceManager._GetAskedPicture());
    }
}
