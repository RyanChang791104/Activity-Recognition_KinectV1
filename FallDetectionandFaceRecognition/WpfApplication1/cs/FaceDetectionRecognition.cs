using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV;
using LibSVMsharp.Helpers;
using LibSVMsharp;
using Accord.Imaging;
namespace KinectFaceRecognition
{
    public class FaceDetectionRecognition
    {
   
        public CascadeClassifier front = new CascadeClassifier(@"Cascades\haarcascade_frontalface_alt_tree.xml");
        public CascadeClassifier Mouth = new CascadeClassifier(@"Cascades\haarcascade_mcs_mouth.xml");

        private const int FaceDataWidth = 240;
        private const int FaceDataHeight = 320;        

        public Image<Gray, byte> GetDetectedFace(byte[] pixelData, int height, int width)
        {
            var bitmap = BytesToBitmap(pixelData, height, width);
            var image = new Image<Bgr, byte>(bitmap);
            var grayImage = image.Convert<Gray, Byte>();
      
           var frontfacesDetected = front.DetectMultiScale(grayImage, 1.2, 3, new Size(grayImage.Width / 24, grayImage.Width / 24), new Size(grayImage.Width , grayImage.Height ));
      
 
           
            foreach (var frontfacesfaceFound in frontfacesDetected)
            {
              
              //  var face = image.Copy(frontfacesfaceFound).Convert<Gray, byte>();
               var face = image.Copy(frontfacesfaceFound).Convert<Gray, byte>();
            //   face._EqualizeHist();
               face.Save(@"C:\Users\temp\Desktop\facebitmap.jpg");
               Console.WriteLine("face.Height: " + face.Height + "face.Width: " + face.Width);
                
             //  face._EqualizeHist();
              // IsMouthDetected(face);
                return face;
                
            }
           
            return null;
        }

        public bool IsMouthDetected(Image<Gray, byte> face)
        {
            var detectRectangle = new Rectangle(0, face.Height * 2 / 3, face.Width, face.Height / 3);
            var whereMouthShouldBe = face.GetSubRect(detectRectangle);
            var mouths = Mouth.DetectMultiScale(whereMouthShouldBe, 1.2, 10, new Size(5, 5), new Size(5, 5));
       
            return mouths.Any();
        }
      
      
       // public Image<Gray, Byte> Resizegray(Image<Gray, Byte> im)
      //  {
            //return im.Resize(120, 120, Emgu.CV.CvEnum.INTER.CV_INTER_LINEAR);
        //}
        List<float> Finaldata = new List<float>();
        List<int[]> pixel1 = new List<int[]>();

        public void Feature_extraction()
        {

            Image<Bgr, byte> inputimage = new Image<Bgr, byte>(@"C:\Users\temp\Desktop\facebitmap.jpg");
            FileStream fs = new FileStream(@"C:\Users\temp\Desktop\Face_feature.txt", FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs);
            LocalBinaryPattern lbp = new Accord.Imaging.LocalBinaryPattern(3, 6, true);
            Image<Gray, byte> gray = inputimage.Convert<Gray, byte>();
           // Image<Gray, byte> newgray = Resizegray(gray);
         //   newgray._EqualizeHist();
           
         //   Image<Gray, byte> mask = new Image<Gray, byte>(newgray.Width, newgray.Height);
           // Image<Gray, byte> dest = new Image<Gray, byte>(newgray.Width, newgray.Height);
           // CvInvoke.cvEllipse(mask, new Point(55, 50), new Size(52, 80), 0, 0, 360, new MCvScalar(256, 256, 256), -1, Emgu.CV.CvEnum.LINE_TYPE.CV_AA, 0);
          //  dest = newgray.And(newgray, mask);
          //  var newdest = dest.GetSubRect(new Rectangle(new Point(20, 23), new Size(80, 84)));
            //newdest.SmoothBilatral(0, 20, 2);
            
           // lbp.ProcessImage(newdest.Bitmap);


            for (int i = 0; i < 13; i++)
            {
                for (int j = 0; j < 14; j++)
                {
                    pixel1.Add(lbp.Histograms[i, j]);
                }
            }

            //   Matrix<float> pixeldata = new Matrix<float>(256, 256);

            for (int j = 0; j < 13 * 14; j++)
            {
                var pixe = pixel1[j];
                for (int i = 0; i < 256; i++)
                {
                    // lbpdata[0, i] += pixe[i];
                    Finaldata.Add(pixe[i]);
                }
            }
            //  CvInvoke.cvNormalize(pixeldata, pixeldata, 1, -1, NORM_TYPE.CV_L2, empty);

            string str = " ";

            sw.Write((4) + " ");

            for (int j = 0; j < 182* 256; j++)
            {
                sw.Write((j + 1) + ":" + Finaldata[j] + str);
                sw.Write("");
            }

            sw.WriteLine();


            sw.Close();
            pixel1.Clear();
            Finaldata.Clear();
            /*
            for (int o = 0; o <64*256; o++)
            {
                SVMNode node = new SVMNode();
                node.Index = Convert.ToInt32(o);
                node.Value = Convert.ToDouble(Finaldata[o]);
                nodes.Add(node);
            }
            problem.Add(nodes.ToArray(), 1);
            pixel1.Clear();
            Finaldata.Clear();
            return problem;
           */
        }
    
        public  Bitmap BytesToBitmap(byte[] pixelData, int height, int width)
        {
            var bitmap = new Bitmap(width, height, PixelFormat.Format32bppRgb);
            var bitmapData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, bitmap.PixelFormat);
            var ptr = bitmapData.Scan0;
            Marshal.Copy(pixelData, 0, ptr, pixelData.Length);
            bitmap.UnlockBits(bitmapData);
            return bitmap;
        }
     

  

      
    }
}