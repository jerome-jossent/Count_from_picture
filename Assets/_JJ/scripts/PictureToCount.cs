using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnityExample;
using System;

public class PictureToCount
{
    public static Mat displayImage;
    public static Mat contours_mask;

    public static int contours_filtered_count;

    public enum Seuillage_type { manuel, adaptative }

    internal static void Compute(Mat rgbaMat,
        Seuillage_type seuillage_Type,
        double threshold_thresh,
        float seuilMin_float,
        float seuilMax_float,
        bool drawContours,
        SourceManager.Output_type output_type,
        Color contour_Color,
        Color internal_Color)
    {
        if (rgbaMat == null)
        {
            Debug.Log("rgbaMat vide");
            return;
        }

        displayImage = new Mat();
        rgbaMat.copyTo(displayImage);

        int area_tot = rgbaMat.cols() * rgbaMat.rows();
        int seuilMin = (int)(area_tot * seuilMin_float / 100);
        int seuilMax = (int)(area_tot * seuilMax_float / 100);
        //Debug.Log($"{seuilMin}\t{seuilMax}\t[{area_tot}]");

        // Convert image to grayscale
        Mat gray = new Mat();
        Imgproc.cvtColor(rgbaMat, gray, Imgproc.COLOR_BGR2GRAY);

        // Convert image to binary
        Mat bw = new Mat();
        switch (seuillage_Type)
        {
            case Seuillage_type.manuel:
                Imgproc.threshold(gray, bw, threshold_thresh, 255, 0);
                break;
            case Seuillage_type.adaptative:
                Imgproc.adaptiveThreshold(gray, bw, 255, Imgproc.ADAPTIVE_THRESH_GAUSSIAN_C, Imgproc.THRESH_BINARY, 11, 2);
                break;
        }

        switch (output_type)
        {
            default:
            case SourceManager.Output_type.original: break;
            case SourceManager.Output_type.gray: Imgproc.cvtColor(gray, displayImage, Imgproc.COLOR_GRAY2RGBA); break;
            case SourceManager.Output_type.binary: Imgproc.cvtColor(bw, displayImage, Imgproc.COLOR_GRAY2RGBA); break;
        }

        // Find all the contours in the thresholded image
        Mat hierarchy = new Mat();
        List<MatOfPoint> contours = new List<MatOfPoint>();
        Imgproc.findContours(bw, contours, hierarchy, Imgproc.RETR_LIST, Imgproc.CHAIN_APPROX_NONE);

        Scalar couleur_contour = new Scalar(contour_Color.r * 255, contour_Color.g * 255, contour_Color.b * 255, contour_Color.a * 255);
        Scalar couleur_interne = new Scalar(internal_Color.r * 255, internal_Color.g * 255, internal_Color.b * 255, internal_Color.a * 255);

        //Count (and draw) filtered contours
        contours_filtered_count = 0;

        List<MatOfPoint> filtered_contours_to_draw = new List<MatOfPoint>();
        for (int i = 0; i < contours.Count; ++i)
        {
            // Calculate the area of each contour
            double area = Imgproc.contourArea(contours[i]);

            // Ignore contours that are too small or too large
            if (area < seuilMin || seuilMax < area)
                continue;

            contours_filtered_count++;
            if (drawContours)
            {
                if (internal_Color.a > 0 || contour_Color.a > 0)
                    filtered_contours_to_draw.Add(contours[i]);
                //if (internal_Color.a > 0)
                //    Imgproc.drawContours(displayImage, contours, i, new Scalar(0, 200, 0, 255), -1);
                //if (contour_Color.a > 0)
                //    Imgproc.drawContours(displayImage, contours, i, new Scalar(0, 100, 0, 255), 2);
            }           

            #region old OpenCV code
            ////Construct a buffer used by the pca analysis
            //List<Point> pts = contours[i].toList();
            //int sz = pts.Count;
            //Mat data_pts = new Mat(sz, 2, CvType.CV_64FC1);
            //for (int p = 0; p < data_pts.rows(); ++p)
            //{
            //    data_pts.put(p, 0, pts[p].x);
            //    data_pts.put(p, 1, pts[p].y);
            //}

            //Mat mean = new Mat();
            //Mat eigenvectors = new Mat();
            //Core.PCACompute(data_pts, mean, eigenvectors, 1);
            ////Debug.Log("mean.dump() " + mean.dump());
            ////Debug.Log("eigenvectors.dump() " + eigenvectors.dump());

            //Point cntr = new Point(mean.get(0, 0)[0], mean.get(0, 1)[0]);
            //Point vec = new Point(eigenvectors.get(0, 0)[0], eigenvectors.get(0, 1)[0]);

            ////drawAxis(src, cntr, vec, new Scalar(255, 255, 0), 150);

            //data_pts.Dispose();
            //mean.Dispose();
            //eigenvectors.Dispose();
            #endregion
        }

        contours_mask = new Mat(rgbaMat.rows(), rgbaMat.cols(), rgbaMat.type(), new Scalar(0, 0, 0, 0)); //RGBA
        if (filtered_contours_to_draw.Count > 0)
        {
            //create mask
            //Mat contours_mask = new Mat(rgbaMat.rows(), rgbaMat.cols(), CvType.CV_8UC4, new Scalar(0, 0, 0, 0));            

            for (int i = 0; i < filtered_contours_to_draw.Count; i++)
            {
                // Draw each contour only for visualisation purposes
                if (internal_Color.a > 0)
                    Imgproc.drawContours(contours_mask, filtered_contours_to_draw, i, couleur_interne, -1);
                if (contour_Color.a > 0)
                    Imgproc.drawContours(contours_mask, filtered_contours_to_draw, i, couleur_contour, 2);
            }

            //displayImage = contours_mask;
//            Core.addWeighted(displayImage, 1, contours_mask, 1, -0.5, displayImage);

            #region from EMGU.CV
            ////Incruste overlay dans image à l'endroit demandé
            //Size taille = cibleMAT.Size;
            //Point point = new Point(x - taille.Width / 2, y - taille.Height / 2);
            //image.ROI = new Rectangle(point, taille);
            //CvInvoke.Subtract(image, mask, image);
            //CvInvoke.AddWeighted(image, 1, overlay, 1, -0.5, image);
            //image.ROI = Rectangle.Empty;
            #endregion
        }
    }
}