using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
public class SourceManager : MonoBehaviour
{
    ScriptsManager _sm;
    public Mat _source_Mat;
    public Parametre threshold_thresh_valeur;
    public Parametre threshold_surf_min_valeur;
    public Parametre threshold_surf_max_valeur;

    public enum Output_type { original, gray, binary }
    public Output_type output_Type;
    public TMPro.TMP_Dropdown output_Type_dropdown;

    public PictureToCount.Seuillage_type seuillage_Type;
    public TMPro.TMP_Dropdown seuillage_Type_dropdown;

    public UnityEngine.UI.Toggle augmentedToggle;

    public TMPro.TMP_Text count_text;

    private void Awake()
    {
        _sm = GameObject.Find("ScriptsManager").GetComponent<ScriptsManager>();

        output_Type_dropdown.options.Clear();
        foreach (var item in Enum.GetValues(typeof(Output_type)))
            output_Type_dropdown.options.Add(new TMPro.TMP_Dropdown.OptionData(item.ToString()));
        output_Type_dropdown.value = 0;
        _output_Type_dropdown_Change();

        seuillage_Type_dropdown.options.Clear();
        foreach (var item in Enum.GetValues(typeof(PictureToCount.Seuillage_type)))
            seuillage_Type_dropdown.options.Add(new TMPro.TMP_Dropdown.OptionData(item.ToString()));
        seuillage_Type_dropdown.value = 0;
        _seuillage_Type_dropdown_Change();
    }

    public void _output_Type_dropdown_Change()
    {
        output_Type = (Output_type)output_Type_dropdown.value;
        _Compute();
    }

    public void _seuillage_Type_dropdown_Change()
    {
        seuillage_Type = (PictureToCount.Seuillage_type)seuillage_Type_dropdown.value;
        switch (seuillage_Type)
        {
            case PictureToCount.Seuillage_type.manuel:
                threshold_thresh_valeur.gameObject.SetActive(true);
                break;
            case PictureToCount.Seuillage_type.adaptative:
                threshold_thresh_valeur.gameObject.SetActive(false);
                break;
        }
        _Compute();
    }

    public void _Compute()
    {
        _sm._imageProcess.ComputePicture();
    }
}