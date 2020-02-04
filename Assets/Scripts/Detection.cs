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

    public int biggestContourIndexShield = -1;
    public int biggestContourIndexSword = -1;

    public bool boolAttack = false;
    public Point lastCenter;

    public bool Block()
    {
        if (biggestContourIndexShield == -1)
            return false;
        
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

        for(int i = 0; i < swordContour.Size ; i++)
        {
            x += swordContour[i].X;
            y += swordContour[i].Y;
        }

        x /= swordContour.Size;
        y /= swordContour.Size;

        newCenter = new Point((int)x,(int)y);

        float distance = Mathf.Sqrt( (lastCenter.X - newCenter.X)*(lastCenter.X - newCenter.X) + (lastCenter.Y  - newCenter.Y )*(lastCenter.Y - newCenter.Y)) ;


        if (swordContourArea < 1000)
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
        UpdateTexture();        
    }

    private void UpdateTexture()
    {
        frame = webcam.QueryFrame();
        texture.LoadRawTextureData(frame.ToImage<Bgra, Byte>().Bytes);
        texture.Apply();
    }

    void OnDestroy()
    {
        webcam.Stop();
    }


    private Mat ProcessImage()
    {
        frame = webcam.QueryFrame();    

        //Focus on the shield
        Mat frameHsvMat = new Mat();
        CvInvoke.CvtColor(frame,frameHsvMat,ColorConversion.Bgr2Hsv);

        Image<Hsv, byte> frameHSV = frameHsvMat.ToImage<Hsv, byte>();

        //Detection of the shield
        Hsv lowerShield = new Hsv(hShieldMin, sShieldMin, vShieldMin);
        Hsv upperShield = new Hsv(hShieldMax, sShieldMax, vShieldMax);

       
        Mat frameShield = frameHSV.InRange(lowerShield, upperShield).Mat;

        Mat structuringElement = CvInvoke.GetStructuringElement(ElementShape.Cross, new Size(2 * operationSize + 1, 2 * operationSize + 1), new Point(operationSize, operationSize) );

        CvInvoke.Erode(frameShield, frameShield, structuringElement, new Point(-1, -1), nbrIteration, BorderType.Constant, new MCvScalar(0));
        CvInvoke.Erode(frameShield, frameShield, structuringElement, new Point(-1, -1), nbrIteration, BorderType.Constant, new MCvScalar(0));

        FindContour(frameShield,"Shield");

        //Detection of the sword

        Hsv lowerSword = new Hsv(hSwordMin,sSwordMin,vSwordMin);
        Hsv upperSword = new Hsv(hSwordMax,sSwordMax,vSwordMax);

        Mat frameSword = frameHSV.InRange(lowerSword, upperSword).Mat;

        CvInvoke.Dilate(frameSword, frameSword, structuringElement, new Point(-1, -1), nbrIteration, BorderType.Constant, new MCvScalar(0));
        CvInvoke.Erode(frameSword, frameSword, structuringElement, new Point(-1, -1), nbrIteration, BorderType.Constant, new MCvScalar(0));

        FindContour(frameSword,"Sword");

        //Focus on the sword
        return frameShield;
    }

    private void FindContour(Mat frame,String nameObj)
    {
        VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
        VectorOfPoint biggestContour = new VectorOfPoint();
        int biggestContourIndex = -1;

        if (nameObj == "Shield")
            biggestContourIndexShield = -1;

        if (nameObj == "Sword")
            biggestContourIndexSword = -1;

        double biggestContourArea;

        Mat hierarchy = new Mat();
        CvInvoke.FindContours(frame,contours,hierarchy, RetrType.List, ChainApproxMethod.ChainApproxNone);

        //gather the one with the bggest areas and its properties;
        for (int i = 0; i < contours.Size; i++)
        {
            biggestContour = contours[i];
            biggestContourIndex = i;

            if (nameObj == "Shield")
            {
                biggestContourIndexShield = i;
                shieldContourArea = CvInvoke.ContourArea(contours[i]);
            }


            if (nameObj == "Sword")
            {
                biggestContourIndexSword = i;
                swordContourArea = CvInvoke.ContourArea(contours[i]);
            }

            biggestContourArea = CvInvoke.ContourArea(contours[i]);
        }

        if (biggestContourIndex > -1)
        {
            if (nameObj == "Shield")
            {

                //Debug.Log("bg "+ CvInvoke.ContourArea(biggestContour));
                shieldContour = biggestContour;

                //Debug.Log("sc "+ CvInvoke.ContourArea(shieldContour));
            }
            else if (nameObj == "Sword")
            {
                swordContour = biggestContour;
            }
            
            CvInvoke.DrawContours(frame, contours, biggestContourIndex, new MCvScalar(0, 0, 255),10);
        }
    }

    private void DisplayCamera()
    {
        webcam = new VideoCapture(0);
        frame = webcam.QueryFrame();

        if (!frame.IsEmpty)
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
    }
}
