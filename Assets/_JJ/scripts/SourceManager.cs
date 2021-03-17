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

    public enum Output_type { original, gray, binary, augmented }
    public Output_type output_Type;
    public TMPro.TMP_Dropdown output_Type_dropdown;

    public PictureToCount.Seuillage_type seuillage_Type;
    public TMPro.TMP_Dropdown seuillage_Type_dropdown;

    public TMPro.TMP_Text count_text;

    private void Awake()
    {
        _sm = GameObject.Find("ScriptsManager").GetComponent<ScriptsManager>();
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
    }

    public void _seuillage_Type_dropdown_Change()
    {
        seuillage_Type = (PictureToCount.Seuillage_type)seuillage_Type_dropdown.value;
    }

    public void _output_Type_dropdown_Change()
    {
        output_Type = (Output_type)output_Type_dropdown.value;
    }

    internal Mat _GetAskedPicture()
    {
        Mat IHM = new Mat();
        switch (output_Type)
        {
            default:
            case Output_type.original: IHM = PictureToCount.original; break;
            case Output_type.gray: Imgproc.cvtColor(PictureToCount.gray, IHM, Imgproc.COLOR_GRAY2RGBA); break;
            case Output_type.binary: Imgproc.cvtColor(PictureToCount.bw, IHM, Imgproc.COLOR_GRAY2RGBA); break;
            case Output_type.augmented: IHM = PictureToCount.augmented; break;
        }
        return IHM;
    }
}