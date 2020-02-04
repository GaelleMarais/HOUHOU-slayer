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

public class TestDetection : MonoBehaviour
{
    private VideoCapture _webcam;
    public RawImage display;
    private Texture2D texture;

    private float width;
    private float height;

    // Start is called before the first frame update
    void Start()
    {
        DisplayCamera();
    }

    // Update is called once per frame
    void Update()
    {
        Mat image = _webcam.QueryFrame();
        texture.LoadRawTextureData(image.ToImage<Bgra, Byte>().Bytes);
        texture.Apply();
        CvInvoke.Imshow("test", image);
    }

    private void DisplayCamera()
    {
        _webcam = new VideoCapture(0);
        Mat frame = _webcam.QueryFrame();

        if (!frame.IsEmpty)
        {
            if (frame.IsContinuous)
            {
                width = display.rectTransform.rect.width;
                height = display.rectTransform.rect.height;

                texture = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false);
                CvInvoke.Resize(frame, frame, new Size((int)width, (int)height));
                CvInvoke.CvtColor(frame, frame, ColorConversion.Bgr2Rgba);

                texture.LoadRawTextureData(frame.ToImage<Rgba, Byte>().Bytes);
                texture.Apply();

                display.texture = texture;
                Debug.Log("Start camera OK");
            }
        }
    }
}
