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
        // _sm._flexibleColorPicker_contour._OnNewColor.AddListener(ComputePicture);
        // _sm._flexibleColorPicker_interne._OnNewColor.AddListener(ComputePicture);
    }

    public void ComputePicture()
    {
        //image process
        PictureToCount.Compute(_sm._webcam_Manager._webcam_rgbaMat,
                               _sm._sourceManager.seuillage_Type,
                               _sm._sourceManager.threshold_thresh_valeur._valeur,
                               _sm._sourceManager.threshold_surf_min_valeur._valeur,
                               _sm._sourceManager.threshold_surf_max_valeur._valeur,
                               _sm._sourceManager.augmentedToggle.isOn,
                               _sm._sourceManager.output_Type,
                               _sm._flexibleColorPicker_contour.color,
                               _sm._flexibleColorPicker_interne.color
                               );
        
        //diplay count
        _sm._sourceManager.count_text.text = PictureToCount.contours_filtered_count.ToString();

        //display image
        _sm._webcam_Manager._SetPicture(PictureToCount.displayImage);
        _sm._webcam_Manager._SetOverlay(PictureToCount.contours_mask);
    }
}
