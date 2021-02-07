using OpenCVForUnity.CoreModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CountManager : MonoBehaviour
{
    ScriptsManager _sm;

    private void Awake()
    {
        _sm = GameObject.Find("ScriptsManager").GetComponent<ScriptsManager>();
    }
    public Mat mat;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        //PictureToCount.Compute(rgbaMat, seuillage_Type, threshold_thresh_valeur._valeur);

        //mat = null;
        //switch (output_Type)
        //{
        //    case Output_type.original:
        //        mat = PictureToCount.original;
        //        break;
        //    case Output_type.gray:
        //        mat = new Mat();
        //        Imgproc.cvtColor(PictureToCount.gray, mat, Imgproc.COLOR_GRAY2RGB);
        //        break;
        //    case Output_type.binary:
        //        mat = new Mat();
        //        Imgproc.cvtColor(PictureToCount.bw, mat, Imgproc.COLOR_GRAY2RGB);
        //        break;
        //    case Output_type.augmented:
        //        mat = PictureToCount.augmented;
        //        break;
        //}

        ////string message = PictureToCount.contours_filtered_count + " / " + PictureToCount.contours.Count;
        //string message = PictureToCount.contours_filtered_count.ToString();

        ////Imgproc.putText(HMI, message,
        ////    new Point(5, rgbaMat.rows() - 10), Imgproc.FONT_HERSHEY_SIMPLEX,
        ////    1.0, new Scalar(255, 0, 0, 0), 2, Imgproc.LINE_AA, false);

        //count_text.text = message;

    }
    //public void _output_Type_dropdown_Change()
    //{
    //    TMPro.TMP_Dropdown.OptionData od = output_Type_dropdown.options[output_Type_dropdown.value];
    //    Enum.TryParse(od.text, out output_Type);
    //}
    //public void _seuillage_Type_dropdown_Change()
    //{
    //    TMPro.TMP_Dropdown.OptionData od = seuillage_Type_dropdown.options[seuillage_Type_dropdown.value];
    //    Enum.TryParse(od.text, out seuillage_Type);
    //}

    //public void _stream_on_switch()
    //{
    //    stream_on = !stream_on;
    //}
}
