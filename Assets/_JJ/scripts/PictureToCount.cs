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
    static float nextTime;
    public static Mat original;
    public static Mat gray;
    public static Mat bw;
    public static Mat augmented;

    public static List<MatOfPoint> contours;
    public static int contours_filtered_count;

    public enum Seuillage_type { manuel, adaptative}

    internal static void Compute(Mat rgbaMat, Seuillage_type seuillage_Type, double threshold_thresh, float seuilMin_float, float seuilMax_float)
    {
        if (rgbaMat == null)
        {
            Debug.Log("vide");
            return;
        }
        original = new Mat();
        rgbaMat.copyTo(original);

        augmented = new Mat();
        rgbaMat.copyTo(augmented);

        // Convert image to grayscale
        gray = new Mat();
        Imgproc.cvtColor(augmented, gray, Imgproc.COLOR_BGR2GRAY);
        // Convert image to binary
        bw = new Mat();

        switch (seuillage_Type)
        {
            case Seuillage_type.manuel:
                Imgproc.threshold(gray, bw, threshold_thresh, 255, 0);
                break;
            case Seuillage_type.adaptative:
                Imgproc.adaptiveThreshold(gray, bw, 255, Imgproc.ADAPTIVE_THRESH_GAUSSIAN_C, Imgproc.THRESH_BINARY, 11, 2);
                break;
        }

        // Find all the contours in the thresholded image
        Mat hierarchy = new Mat();
        contours = new List<MatOfPoint>();
        Imgproc.findContours(bw, contours, hierarchy, Imgproc.RETR_LIST, Imgproc.CHAIN_APPROX_NONE);

        int area_tot = rgbaMat.cols() * rgbaMat.rows();
        int seuilMin = (int)(area_tot * seuilMin_float/100);
        int seuilMax = (int)(area_tot * seuilMax_float/100);

        Debug.Log($"{seuilMin}\t{seuilMax}\t[{area_tot}]");

        contours_filtered_count = 0;
        for (int i = 0; i < contours.Count; ++i)
        {
            // Calculate the area of each contour
            double area = Imgproc.contourArea(contours[i]);
            // Ignore contours that are too small or too large
            if (area < seuilMin || seuilMax < area)
                continue;

            contours_filtered_count++;
            // Draw each contour only for visualisation purposes
            Imgproc.drawContours(augmented, contours, i, new Scalar(255, 0, 0, 128), 2);

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
        }



    }
}