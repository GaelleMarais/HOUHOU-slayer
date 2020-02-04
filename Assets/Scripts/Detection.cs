using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Emgu.CV;
using Emgu.CV.Util;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

using System;
using System.Drawing;
using System.IO;
using System.Numerics;
using UnityEngine.UI;
using ZedGraph;


public class Detection : MonoBehaviour
{
    private VideoCapture webcam;

    public Image img;
    public RawImage display;
    public Texture2D texture;
    private float width;
    private float height;

    public double hShieldMin;
    public double sShieldMin;
    public double vShieldMin;

    public double hShieldMax = 179;
    public double sShieldMax = 255;
    public double vShieldMax = 255;

    public double hSwordMin;
    public double sSwordMin;
    public double vSwordMin;

    public double hSwordMax = 179;
    public double sSwordMax = 255;
    public double vSwordMax = 255;

    public int operationSize = 1;
    public int nbrIteration = 2;

    public VectorOfPoint shieldContour = new VectorOfPoint();
    public double shieldContourArea = 0;

    public VectorOfPoint swordContour = new VectorOfPoint();
    public double swordContourArea = 0;

    private Mat frame;
    private Mat binaryShield;
    private Mat binarySword;

    public int biggestContourIndexShield = -1;
    public int biggestContourIndexSword = -1;

    public bool boolAttack = false;
    public Point lastCenter;

    public bool Block()
    {
        if (shieldContourArea > 15000)
        { 
            //Debug.Log(CvInvoke.ContourArea(shieldContour));
            return true;
        }
        else
        {
            //Debug.Log(CvInvoke.ContourArea(shieldContour));
            return false;
        }
    }

    public bool Attack()
    {
        //return false;
        //Debug.Log(boolAttack);
        return boolAttack;
    }

    public void CheckAttack()
    {
        Point newCenter;
        float x = 0;
        float y = 0;

        if(swordContour != null)
        {
            for (int i = 0; i < swordContour.Size; i++)
            {
                if(swordContour[i] != null)
                {
                    x += swordContour[i].X;
                    y += swordContour[i].Y;
                }
            }

            x /= swordContour.Size;
            y /= swordContour.Size;
        }
        
          
        newCenter = new Point((int)x,(int)y);

        float distance = Mathf.Sqrt( (lastCenter.X - newCenter.X)*(lastCenter.X - newCenter.X) + (lastCenter.Y  - newCenter.Y )*(lastCenter.Y - newCenter.Y)) ;


        if (swordContourArea < 500)
        {
            boolAttack = false;
        }
        else 
        {
            if (distance > 150)
            {
                boolAttack = true;
            }
            else
            {
                boolAttack = false;
            }
        }       

        // Debug.Log("dist =" + distance + "  v= " + boolAttack);

        lastCenter = newCenter;
        Invoke("CheckAttack",0.5f);
    }

    // Start is called before the first frame update
    void Start()
    {        
        DisplayCamera();
        CheckAttack();
    }

    // Update is called once per frame
    void Update()
    {
        ProcessImage();
        CvInvoke.Resize(frame, frame, new Size(), 0.5f, 0.5f, Inter.Linear);
        CvInvoke.Imshow("camera", frame); 
    }


    void OnDestroy()
    {
        webcam.Stop();
    }


    private void ProcessImage()
    {
        frame = webcam.QueryFrame();    

        //Focus on the shield
        Mat frameHsvMat = new Mat();
        CvInvoke.CvtColor(frame,frameHsvMat,ColorConversion.Bgr2Hsv);


        Image<Hsv, byte> frameHSV = frameHsvMat.ToImage<Hsv, byte>();

        //Detection of the shield
        Hsv lowerShield = new Hsv(hShieldMin, sShieldMin, vShieldMin);
        Hsv upperShield = new Hsv(hShieldMax, sShieldMax, vShieldMax);

       
        binaryShield = frameHSV.InRange(lowerShield, upperShield).Mat;

        Mat structuringElement = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(2 * operationSize + 1, 2 * operationSize + 1), new Point(operationSize, operationSize) );

        CvInvoke.Dilate(binaryShield, binaryShield, structuringElement, new Point(-1, -1), nbrIteration, BorderType.Constant, new MCvScalar(0));
        CvInvoke.Erode(binaryShield, binaryShield, structuringElement, new Point(-1, -1), nbrIteration, BorderType.Constant, new MCvScalar(0));

        FindContourShield();

        //Detection of the sword

        Hsv lowerSword = new Hsv(hSwordMin,sSwordMin,vSwordMin);
        Hsv upperSword = new Hsv(hSwordMax,sSwordMax,vSwordMax);

        binarySword = frameHSV.InRange(lowerSword, upperSword).Mat;

        CvInvoke.Dilate(binarySword, binarySword, structuringElement, new Point(-1, -1), nbrIteration, BorderType.Constant, new MCvScalar(0));
        CvInvoke.Erode(binarySword, binarySword, structuringElement, new Point(-1, -1), nbrIteration, BorderType.Constant, new MCvScalar(0));

        FindContourSword();

    }

    private void FindContourShield()
    {
        VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
        VectorOfPoint biggestContour = new VectorOfPoint();

        int biggestContourIndex = -1;
        double biggestContourArea = 0;

        Mat hierarchy = new Mat();
        CvInvoke.FindContours(binaryShield, contours, hierarchy, RetrType.List, ChainApproxMethod.ChainApproxNone);

        //gather the one with the biggest areas and its properties;
        for (int i = 0; i < contours.Size; i++)
        {
            if (biggestContourArea < CvInvoke.ContourArea(contours[i]))
            {
                biggestContour = contours[i];
                biggestContourIndex = i;
             
                biggestContourArea = CvInvoke.ContourArea(contours[i]);
             
            }
        }

        if (biggestContourIndex > -1)
        {
            //Debug.Log("bg "+ CvInvoke.ContourArea(biggestContour));
            //Debug.Log("sc "+ CvInvoke.ContourArea(shieldContour));

            shieldContourArea = biggestContourArea;
            shieldContour = biggestContour;
            CvInvoke.DrawContours(binaryShield, contours, biggestContourIndex, new MCvScalar(0, 0, 255), 10);
            CvInvoke.Resize(binaryShield, binaryShield, new Size(), 0.5f, 0.5f, Inter.Linear);
            CvInvoke.Imshow("shield", binaryShield);
        }
    }

    private void FindContourSword()
    {
        VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
        VectorOfPoint biggestContour = new VectorOfPoint();

        int biggestContourIndex = -1;
        double biggestContourArea = 0;

        Mat hierarchy = new Mat();
        CvInvoke.FindContours(binarySword, contours, hierarchy, RetrType.List, ChainApproxMethod.ChainApproxNone);

        //gather the one with the biggest areas and its properties;
        for (int i = 0; i < contours.Size; i++)
        {
            if (biggestContourArea < CvInvoke.ContourArea(contours[i]))
            {
                biggestContour = contours[i];
                biggestContourIndex = i;

                biggestContourArea = CvInvoke.ContourArea(contours[i]);

            }
        }

        if (biggestContourIndex > -1)
        {
            //Debug.Log("bg "+ CvInvoke.ContourArea(biggestContour));
            //Debug.Log("sc "+ CvInvoke.ContourArea(shieldContour));

            swordContourArea = biggestContourArea;
            swordContour = biggestContour;
            CvInvoke.DrawContours(binarySword, contours, biggestContourIndex, new MCvScalar(0, 255, 0), 10);
            CvInvoke.Resize(binarySword, binarySword, new Size(), 0.5f, 0.5f, Inter.Linear);
            CvInvoke.Imshow("sword", binarySword);
        }
    }

    private void DisplayCamera()
    {
        webcam = new VideoCapture(0);
        frame = webcam.QueryFrame();
        CvInvoke.Imshow("camera", frame);

        /* if (!frame.IsEmpty)
        {
            if (frame.IsContinuous)
            {
                width = display.rectTransform.rect.width;
                height = display.rectTransform.rect.height;

                texture = new Texture2D(webcam.Width, webcam.Height, TextureFormat.BGRA32, false);

                texture.LoadRawTextureData(frame.ToImage<Bgra, Byte>().Bytes);
                texture.Apply();

                display.texture = texture;
                Debug.Log("Start camera OK");
            }
        }
        */
    }
}
