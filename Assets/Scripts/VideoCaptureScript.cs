using UnityEngine;
using UnityEngine.UI;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

using Emgu.CV;
using Emgu.CV.Util;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;



public class VideoCaptureScript : MonoBehaviour
{
    VideoCapture video;
    public RawImage display;
    public Texture2D texture;
    [Range(0, 255)]
    public double hmin;
    [Range(0, 255)]
    public double hmax;
    [Range(0, 255)]
    public double smin;
    [Range(0, 255)]
    public double smax;
    [Range(0, 255)]
    public double vmin;
    [Range(0, 255)]
    public double vmax;


    // Start is called before the first frame update
    void Start()
    {
        // video = new VideoCapture("Assets/Videos/test_camera.flv");
        video = new VideoCapture(0);
    }

    // Update is called once per frame
    void Update()
    {
        Mat orig;
        orig = video.QueryFrame();
        if(orig != null)
        {
            CvInvoke.WaitKey(24);

            Mat image2 = orig.Clone();
            Mat final = ImageTreatment(image2);
            CvInvoke.Imshow("2", final);

            Mat image3 = final.Clone();
            Mat borders = Borders(final, orig);
            //CvInvoke.Imshow("3", borders);
            DisplayFrame(borders);
        }
        else
        {
            CvInvoke.DestroyAllWindows();
        }

    }

    void OnDestroy()
    {
        CvInvoke.DestroyAllWindows();
    }

     Mat ImageTreatment(Mat image)
    {
        CvInvoke.CvtColor(image, image, ColorConversion.Bgr2Hsv);
        CvInvoke.MedianBlur(image, image, 21);


        Hsv lower = new Hsv(hmin, smin, vmin);
        Hsv higher = new Hsv(hmax, smax, vmax);

        Image<Hsv, Byte> i = image.ToImage<Hsv, Byte>();
        Mat result = i.InRange(lower, higher).Mat;

        int operationSize = 1;

        Mat structuringElement = CvInvoke.GetStructuringElement(ElementShape.Ellipse,
                                                                new Size(2 * operationSize +1, 2 * operationSize + 1),
                                                                new Point(operationSize, operationSize));

        CvInvoke.Erode(result, result, structuringElement, new Point(-1, -1), 1, BorderType.Default, new MCvScalar(0));
        CvInvoke.Dilate(result, result, structuringElement, new Point(-1, -1), 1, BorderType.Default, new MCvScalar(0));

        return result;
    }

    Mat Borders(Mat binary, Mat orig)
    {
        Mat image = orig.Clone();

        VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint(100);
        VectorOfPoint biggestContour = new VectorOfPoint(100);
        int biggestContourIndex = -1;
        double biggestContourArea = 0;

        Mat hierarchy = new Mat();
        CvInvoke.FindContours(binary, contours, hierarchy, RetrType.List, ChainApproxMethod.ChainApproxNone);

        for(int i = 0; i < contours.Size; i++)
        {
            if( CvInvoke.ContourArea(contours[i]) > biggestContourArea)
            {
                biggestContour = contours[i];
                biggestContourIndex = i;
                biggestContourArea = CvInvoke.ContourArea(contours[i]);
            }
        }

        // Draw biggest contour

        // if(biggestContourIndex > -1)
        // {
        //    image = DrawContour(image, contours, biggestContourIndex);
        // }

        // Draw All contours

        for (int i = 0; i < contours.Size; i++)
        {
            image = DrawContour(image, contours, i);
        }

        return image;
    }

    Mat DrawContour(Mat input, VectorOfVectorOfPoint contours, int index)
    {
        Mat image = input.Clone();

        MCvScalar color = new MCvScalar(35, 95, 255);
        CvInvoke.DrawContours(image, contours, index, color, 4);

        var moments = CvInvoke.Moments(contours[index]);
        int cx = (int)(moments.M10 / moments.M00);
        int cy = (int)(moments.M01 / moments.M00);
        Point centroid = new Point(cx, cy);

        CvInvoke.Circle(image, centroid, 4, color, 2);

        return image;
    }

    private void DisplayFrame(Mat frame)
    {
        if (!frame.IsEmpty)
        {
            if (frame.IsContinuous)
            {
                int width = (int)display.rectTransform.rect.width;
                int height = (int)display.rectTransform.rect.height;

                if(texture != null)
                {
                    Destroy(texture);
                    texture = null;
                }

                texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
                CvInvoke.Resize(frame, frame, new Size(width, height));
                CvInvoke.CvtColor(frame, frame, ColorConversion.Bgr2Rgba);

                texture.LoadRawTextureData(frame.ToImage<Rgba, Byte>().Bytes);
                texture.Apply();

                display.texture = texture;
            }
        }
    }

}
