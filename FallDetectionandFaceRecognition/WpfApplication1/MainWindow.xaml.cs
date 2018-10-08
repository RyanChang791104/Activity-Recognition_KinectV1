using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using Microsoft.Kinect;
using System.Drawing.Imaging;
using System.IO;
using System.Timers;
using System.Net.Mail;
using BioGait;
using FUKinectTool;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading.Tasks;
using LibSVMsharp.Helpers;
using LibSVMsharp.Extensions;
using LibSVMsharp;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV;
using System.Collections.Concurrent;
using KinectFaceRecognition;
using System.Threading;
using System.Net.Cache;
using System.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WpfApplication1
{
    public partial class MainWindow : Window 
    {
        //the confinguration of skeleton joints thickness, color, loading model(face and activity)
        #region Variables

        private const float RenderWidth = 640.0f;
        private const float RenderHeight = 480.0f;
        private const double JointThickness = 3;
        private const double BodyCenterThickness = 10;
        private const double ClipBoundsThickness = 10;
        private readonly Brush centerPointBrush = Brushes.Blue;
        private readonly Brush trackedJointBrush = new SolidColorBrush(Color.FromArgb(255, 68, 192, 68));
        private readonly Brush inferredJointBrush = Brushes.Black;
        private readonly Pen trackedBonePen = new Pen(Brushes.Blue, 3);
        private readonly Pen inferredBonePen = new Pen(Brushes.Red, 5);
        private KinectSensor sensor;
        private DrawingGroup drawingGroup;
        private DrawingImage imageSource;
        private WriteableBitmap colorBitmap, depthBitmap;
        private DepthImagePixel[] depthPixels;
        private byte[] colorPixels, colorDepthPixels;
        private Coordinate prevCM = new Coordinate();
        private Coordinate earlyCM = new Coordinate();
        private Coordinate prev_velCM = new Coordinate();
        private DateTime timestampPrev = new DateTime();
        private DateTime timestampEarly = new DateTime();
        List<Image<Gray, byte>> facelist = new List<Image<Gray, byte>>();
        // public CascadeClassifier profile = new CascadeClassifier(@"Cascades\haarcascade_profileface.xml");
        // public CascadeClassifier front = new CascadeClassifier(@"Cascades\haarcascade_frontalface_alt_tree.xml");
        // HaarCascade Face = new HaarCascade(@"Cascades\haarcascade_frontalface_alt_tree.xml");//loading Face Feature
        SVMModel activity_model = SVM.LoadModel(@"Model\activity_model.txt");
        SVMModel face_recognition_model = SVM.LoadModel(@"C:\Users\temp\Desktop\Face_model_1101.txt");
 
        FaceDetectionRecognition facedetection = new FaceDetectionRecognition();
        System.Windows.Threading.DispatcherTimer timertest = new System.Windows.Threading.DispatcherTimer();
        Random randon = new Random();
        #endregion
       
        public MainWindow()
        { 
            InitializeComponent();
            this.WindowState = WindowState.Maximized;
           
        }
      
       

        private void removeface( List<Image<Gray, byte>> list)
        {
            for(int i=0;i<list.Count;i++)
                if (list.Count % 9 == 0)
                {
                    list.RemoveAt(i);
                }

        }
        //display the face
        private BitmapImage getimage(Image<Gray, byte> data)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                data.Bitmap.Save(memory, ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
            }

        }

       
        string[] frontfaceposing = { "Withglass", "Withoutglass", "Happy", "Sad", "Surprised", "Normal", "Sleepy", "Wink", "Leftlight", "Centerlight", "Rightlight" };
     // string[] recognizedname = { "Yu", "Victor", "yaya", "Pam", "Felicia", "Helen", "V", "Fan", "Ryan", "jose", "Wen", "terry", "amie" };
        string[] recognizedname = new string[] { "Felicia", "Helen", "Fan", "Ryan", "amie" };
        int count ,c = 0;
        string facelabel = null;
        int showuptimes = 0;
        double sum=0;
      
     
        private void face_detection(ColorImageFrame colorImageFrame,byte[] pix)
        {
            //get detected face by using the FaceDetectionRecognition cs file 
            Image<Gray, byte> detectedFace = facedetection.GetDetectedFace(pix, colorImageFrame.Height, colorImageFrame.Width);
           
            if (detectedFace != null)
            {
            //add the face to list
                facelist.Add(detectedFace);
                count = facelist.Count % 9;
                 
          //    removeface(facelist);

                  count = count % 9;
               
                if (count == 0) { count = 9;  }
               
                if (count != 0)
                {
                    //return the last face 
                    faceimage.Source = getimage(facelist.Last());
                
                    facedetection.Feature_extraction();
                   
                    facename = SVM_face_recognition();

                    display.FontSize = 20;
                    display.FontStyle = FontStyles.Normal;
                    display.Foreground = Brushes.Red;
                    display.Background = Brushes.Black;
                 // display.Content ="\nface shows: "+faceshow+ "face fail: "+facefail;
                    
                    switch (facename)
                    {
                        case 1:

                            display.Content = "You are " + recognizedname[0] + " at: " + DateTime.Now + " Confidence is: " + f1*100+"%" + "\nface shows: " + faceshow + " face fail: " + facefail;
                            facelabel = recognizedname[0];
                            Console.WriteLine(recognizedname[0]);
                            
                            
                            break;
                        case 2:

                            display.Content = "You are " + recognizedname[1] + " at: " + DateTime.Now + " Confidence is: " + f2 * 100 + "%" + "\nface shows: " + faceshow + " face fail: " + facefail;
                            facelabel = recognizedname[1];
                            Console.WriteLine(recognizedname[1]);
                           
                            break;
                        case 3:

                            display.Content = "You are " + recognizedname[2] + " at: " + DateTime.Now + " Confidence is: " + f3 * 100 + "%" + "\nface shows: " + faceshow + " face fail: " + facefail;
                            facelabel = recognizedname[2];
                            Console.WriteLine(recognizedname[2]);
                          
                            break;
                        case 4:

                            display.Content = "You are " + recognizedname[3] + " at: " + DateTime.Now + " Confidence is: " + f4 * 100 + "%" + "\nface shows: " + faceshow + " face fail: " + facefail;
                            facelabel = recognizedname[3];
                            Console.WriteLine(recognizedname[3]);
                            
                            break;
                        case 5:

                            display.Content = "You are " + recognizedname[4] + " at: " + DateTime.Now + " Confidence is: " + f5 * 100 + "%" + "\nface shows: " + faceshow + " face fail: " + facefail;
                            facelabel = recognizedname[4];
                            Console.WriteLine(recognizedname[4]);
                          
                            break;
                        case 6:

                            display.Content = "You are " + recognizedname[5] + " at: " + DateTime.Now + " Confidence is: " + f6 * 100 + "%" + "\nface shows: " + faceshow + " face fail: " + facefail;
                            facelabel = recognizedname[5];
                            Console.WriteLine(recognizedname[5]);
                         
                            break;
                        case 7:

                            display.Content = "You are " + recognizedname[6] + " at: " + DateTime.Now + " Confidence is: " + f7 * 100 + "%" + "\nface shows: " + faceshow + " face fail: " + facefail;
                            facelabel = recognizedname[6];
                            Console.WriteLine(recognizedname[6]);
                         
                            break;
                        case 8:

                            display.Content = "You are " + recognizedname[7] + " at: " + DateTime.Now + " Confidence is: " + f8 * 100 + "%" + "\nface shows: " + faceshow + " face fail: " + facefail;
                            facelabel = recognizedname[7];
                            Console.WriteLine(recognizedname[7]);
                           
                            break;
                        case 9:

                            display.Content = "You are " + recognizedname[8] + " at: " + DateTime.Now + " Confidence is: " + f9 * 100 + "%" + "\nface shows: " + faceshow + " face fail: " + facefail + " Accuracy: " + sum;
                            facelabel = recognizedname[8];
                            Console.WriteLine(recognizedname[8]);
                 

                            break;
                        case 10:

                            display.Content = "You are " + recognizedname[9] + " at: " + DateTime.Now + " Confidence is: " + f10 * 100 + "%" + "\nface shows: " + faceshow + " face fail: " + facefail;
                            facelabel = recognizedname[9];
                            Console.WriteLine(recognizedname[9]);
                       
                            break;
                        case 11:

                            display.Content = "You are " + recognizedname[10] + " at: " + DateTime.Now + " Confidence is: " + f11 * 100 + "%" + "\nface shows: " + faceshow + " face fail: " + facefail;
                            facelabel = recognizedname[10];
                            Console.WriteLine(recognizedname[10]);
                   
                            break;
                        case 12:

                            display.Content = "You are " + recognizedname[11] + " at: " + DateTime.Now + " Confidence is: " + f12 * 100 + "%" + "\nface shows: " + faceshow + " face fail: " + facefail;
                            facelabel = recognizedname[11];
                            Console.WriteLine(recognizedname[11]);
                       
                            break;
                    }
           
                    switch (count)
                    {
                        case 1:

                            faceimage1.Source = getimage(detectedFace);
                          
                            break;
                        case 2:

                            faceimage2.Source = getimage(detectedFace);
                          
                            break;
                        case 3:

                            faceimage3.Source = getimage(detectedFace);

                            break;
                        case 4:
                           
                            faceimage4.Source = getimage(detectedFace);
                            break;
                        case 5:
                            
                            faceimage5.Source = getimage(detectedFace);
                        
                            break;
                        case 6:
                         
                            faceimage6.Source = getimage(detectedFace);
                         
                            break;
                        case 7:
                      
                            faceimage7.Source = getimage(detectedFace);
                         
                            break;
                        case 8:
                            
                            faceimage8.Source = getimage(detectedFace);
                      
                            break;
                        case 9:
                          
                            faceimage9.Source = getimage(detectedFace);
                      
                            break;
                    }
                }
          
                    c++;
                    count++;
            }


        }
     // when system loaded only happen once 
          private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            if (KinectSensor.KinectSensors.Count == 0)
            {
                MessageBox.Show("No Kinects device detected", "Fall Detection");
                Application.Current.Shutdown();
            }

            // Create the drawing group we'll use for drawing
            drawingGroup = new DrawingGroup();

            // Create an image source that we can use in our image control
            this.imageSource = new DrawingImage(this.drawingGroup);

            // Display the drawing using our image control
            SkeletalImage.Source = imageSource;
     
            // Look through all sensors and start the first connected one.
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    //the parameter make skeleton joints more smooth
                    sensor = potentialSensor;
                    var parameters = new TransformSmoothParameters
                    {
                        Smoothing = 0.3f,
                        Correction = 0.0f,
                        Prediction = 0.0f,
                        JitterRadius = 1.0f,
                        MaxDeviationRadius = 0.5f
                    };
                    this.sensor.SkeletonStream.Enable(parameters);
                    break;
                }
            }

            if (null != this.sensor)
            {


                this.sensor.ColorStream.Enable(ColorImageFormat.RgbResolution1280x960Fps12);

                // Allocate space to put the pixels we'll receive
                this.colorPixels = new byte[this.sensor.ColorStream.FramePixelDataLength];

                // This is the bitmap we'll display on-screen
                this.colorBitmap = new WriteableBitmap(this.sensor.ColorStream.FrameWidth, this.sensor.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);
                
                this.ColorImage.Source = this.colorBitmap;
               // Add an event handler to be called whenever there is new color frame data
           
              // timertest.Interval = new TimeSpan(0, 0, 0, 5);
               sensor.AllFramesReady += AllFramesReady;
               Instructuring_Training.Content = "Please Posing " + frontfaceposing[0];
               Instructuring_Training.FontSize = 14;
               Instructuring_Training.FontStyle = FontStyles.Normal;
               Instructuring_Training.Foreground = Brushes.Red;
               Instructuring_Training.Background = Brushes.Black;

             // sensor.SkeletonStream.Enable();
             // sensor.SkeletonFrameReady += SensorSkeletonFrameReady;
              
            //  configureDepthStream();
            //  sensor.DepthFrameReady += SensorDepthFrameReady;

                // Start the sensor
                try
                { 
                    sensor.Start(); 
                    // Set angle of Kinect
                    sensor.ElevationAngle = -0; 
                }
                catch (IOException)
                { sensor = null; }

            }
        }

        Skeleton[] skeletonData1 = new Skeleton[0];
        //depth stream configuration 
        private void configureDepthStream()
        {
            int frameLength = sensor.DepthStream.FramePixelDataLength;
            // Turn on the depth stream to receive depth frames
            sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
            
            skeletonData1 = new Skeleton[sensor.SkeletonStream.FrameSkeletonArrayLength];
            // Allocate space to put the depth pixels we'll receive
            depthPixels = new DepthImagePixel[frameLength];
            // Allocate space to put the color pixels we'll create
            colorDepthPixels = new byte[frameLength * sizeof(int)];
            // Bitmap that will be displayed
            depthBitmap = new WriteableBitmap(sensor.DepthStream.FrameWidth, sensor.DepthStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);
            DepthImage.Source = depthBitmap;
          
        }

        // Event handler for switching between skeletonframe and colorframe
        void checkBoxSkelOnlyChanged(object sender, RoutedEventArgs e)
        {
            if (checkBoxSkelOnly.IsChecked.GetValueOrDefault())
            { ColorImage.Visibility = Visibility.Hidden; }
            else { ColorImage.Visibility = Visibility.Visible; }
        }

        // Execute shutdown tasks
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (null != this.sensor)
            { sensor.Stop(); }
        }
        
        // Event handler for sensor's DepthFrameReady event
        // depth stream, changed the color depends on distance
        private void SensorDepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
         
            using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {
                if (depthFrame != null)
                {
 
                    depthFrame.CopyDepthImagePixelDataTo(depthPixels);
                    byte[] depthdata = new byte[depthPixels.Length*4];
                    int minDepth = depthFrame.MinDepth;
                    int maxDepth = depthFrame.MaxDepth;

                    for (int i = 0; i < depthPixels.Length; ++i)
                    {
                        short depth = depthPixels[i].Depth;

                        if (depth==-1)
                        {
                            depthdata[i * 4 + 0] = 0;
                            depthdata[i * 4 + 1] = 0;
                            depthdata[i * 4 + 2] = 255;
                            depthdata[i * 4 + 3] = 0;
                        }
                        else if (depth == 0)
                        {
                            depthdata[i * 4 + 0] = 0;
                            depthdata[i * 4 + 1] = 255;
                            depthdata[i * 4 + 2] = 255;
                            depthdata[i * 4 + 3] = 0;
                        }
                        else if (depth >= 800 && depth <= 1800)
                        {
                            depthdata[i * 4 + 0] = 0;
                            depthdata[i * 4 + 1] = 255;
                            depthdata[i * 4 + 2] = 0;
                            depthdata[i * 4 + 3] = 0;
                        }
                        else
                        {
                            depthdata[i * 4 + 0] = 255;
                            depthdata[i * 4 + 1] = 255;
                            depthdata[i * 4 + 2] = 255;
                            depthdata[i * 4 + 3] = 0;
                        }
           
                    }

                    // Write the pixel data into our bitmap
                    depthBitmap.WritePixels(
                        new Int32Rect(0, 0, this.depthBitmap.PixelWidth, this.depthBitmap.PixelHeight),
                        depthdata,
                        depthBitmap.PixelWidth * sizeof(int),
                        0);
                   
                }
            }
        }
        int p, q, w, e, r;
        // the window i talked about last time which dealing the data in frame
        ConcurrentQueue<int> myq = new ConcurrentQueue<int>();
        SVMProblem testSet1 = new SVMProblem();

        // activity classification
        public void SVM_Classification()
        {

            testSet1 = SVMProblemHelper.Load(@"Dataset\ADLfall_test1.txt");
         
            testSet1 = testSet1.Normalize(SVMNormType.L2);
   
            float sum;

            if (testSet1.Length != 0) {

                try
                {
            
                    //var resut = model.Predict(testSet1.X[testSet1.Length - 1]);
                   //  p = Convert.ToInt16(resut);
                    //predict the result using model, return result
                    var result = testSet1.Predict(activity_model);
                    p = Convert.ToInt16(result[0]);
                    //put the result into enqueue
                    myq.Enqueue(p);

                    switch (p)
                    {
                        case 1:
                            q++;

                            break;
                        case 2:
                            w++;

                            break;
                        case 3:
                            e++;

                            break;
                        case 4:
                            r++;

                            break;

                    }

                }
                catch
                {
                }
                // if the collected data is larger than 30
                    if (myq.Count > 30 )
                    {
                        
               //  dequeue the old one
                      myq.TryDequeue(out p);
                      switch (p)
                      {
                          case 1:
                              q--;

                              break;
                          case 2:
                              w--;

                              break;
                          case 3:
                              e--;

                              break;
                          case 4:
                              r--;

                              break;

                      }
                        // proportional
                        sum = q + w + e + r;
                        
                   //   activity.Content = ("Sit down:" + sit_down + "\n" + "Walking" + walkig + "\n" + "Standing" + standing + "\n" + "Fall event" + fallevent);
                        activity.Content = ("Sit down: " + Math.Round(e / sum, 2) * 100 + "%" + "\n" + "Walking: " + Math.Round(q / sum, 2) * 100 + "%" + "\n" + "Standing: " + Math.Round(w / sum, 2) * 100 + "%" + "\n" + "Fall event: " + Math.Round(r / sum, 2) * 100 + "%");
                      //  activity.Content = ("Sit down:" + Math.Round(h / sum, 2) + "\n" + "Walking" + Math.Round(w / sum, 2) + "\n" + "Standing" + Math.Round(q / sum, 2) + "\n" + "Fall event" + Math.Round(r / sum, 2));
                        if (e / sum > 0.5) { label.Content = ("You have sit down"); label.Foreground = Brushes.Red; }
                        else if (q / sum > 0.5) { label.Content = "You are walking"; label.Foreground = Brushes.Red; }
                        else if (w / sum > 0.5) { label.Content = "You are standing"; label.Foreground = Brushes.Red; }
                        else if (r / sum > 0.5) { label.Content = "You fell down"; label.Foreground = Brushes.Red; }
                        
                        activity.FontSize = 20;
                        activity.FontStyle = FontStyles.Normal;
                        activity.Foreground = Brushes.Red;
                        activity.Background = Brushes.Black;
             

                    }
          
            }

         
        }
     
        // loading the code i crawl from github.
        FUKinectHelper Kinecthelper = new FUKinectTool.FUKinectHelper();
        FUPostureDetector PostureDetector = new FUKinectTool.FUPostureDetector();
        FUPostureDetector detectrighthandangle = new FUKinectTool.FUPostureDetector();
        Calculation calculation = new Calculation();
        int trackingID;
        bool first = true;
        Skeleton skeletonTracked = new Skeleton();
        Skeleton[] skeletons = new Skeleton[2];
        SVMProblem face_data = new SVMProblem();
        int facename;
        double f1, f2, f3, f4, f5, f6, f7, f8, f9, f10, f11, f12,f13;
        List<double[]> datalist = new List<double[]>();
        List<SVMNode> nodes = new List<SVMNode>();
       
        int index = 12;
        
        // for training the face,
        public void face_training(SVMProblem f_training)
        {
           
            SVMProblem trainingSet = SVMProblemHelper.Load(@"C:\Users\temp\Desktop\0921_towp.txt");
            SVMProblem testSet =     SVMProblemHelper.Load(@"C:\Users\temp\Desktop\0921_towpt.txt");
 
            trainingSet = trainingSet.Normalize(SVMNormType.L2);
            testSet = testSet.Normalize(SVMNormType.L2);
           
            SVMParameter parameter = new SVMParameter();
            parameter.Type = SVMType.NU_SVC;
            parameter.Kernel = SVMKernelType.SIGMOID;
            parameter.C = 1;
            parameter.Gamma = 1;
            parameter.Probability = true;
            double[] crossValidationResults; 
            int nFold = 10;
            trainingSet.CrossValidation(parameter, nFold, out crossValidationResults);
            double crossValidationAccuracy = trainingSet.EvaluateClassificationProblem(crossValidationResults);
            SVMModel model = trainingSet.Train(parameter);

           
            double[] testResults = testSet.Predict(model);
            int[,] confusionMatrix;
            double testAccuracy = testSet.EvaluateClassificationProblem(testResults, model.Labels, out confusionMatrix);
          
            Training_result.Content = "testAccuracy:" + testAccuracy + "\nCross validation accuracy: " + crossValidationAccuracy + "\nCount " + trainingSet.Y.Count;
            Training_result.FontSize = 14;
            Training_result.FontStyle = FontStyles.Normal;
            Training_result.Foreground = Brushes.Red;
            Training_result.Background = Brushes.Black;
            index++;
        }

        // face recognition from face_data
        int faceshow = 0;
        int facefail = 0;
        List<double[]> prolist = new List<double[]>();
     

        public int SVM_face_recognition()
        {

            SVMProblem face_data = SVMProblemHelper.Load(@"C:\Users\temp\Desktop\Face_feature.txt");
            face_data = face_data.Normalize(SVMNormType.L2);
          
           
                    //using Libsvm package which has api to calculate the probabilty
                    face_data.PredictProbability(face_recognition_model, out prolist);
                 
                    var ok = prolist.ToArray();
                    var v = ok[0];
                  // we have 13 person 
                 
                    int maxconfidenceindex = 0;
                    double maxconfidence = v[maxconfidenceindex];
                    double threshold = 0.25;

                    for (int i = 0; i < v.Count(); i++)
                    {
                        if(v[i]>maxconfidence)
                        {
                        maxconfidenceindex = i;
                        maxconfidence = v[i];
                        }
                    }
                    if (threshold < maxconfidence)
                    {
                        f1 = v[0];
                        f2 = v[1];
                        f3 = v[2];
                        f4 = v[3];
                        f5 = v[4];
                  /*     
                        f6 = v[5];
                        f7 = v[6];
                        f8 = v[7];
                        f9 = v[8];
                        f10 = v[9];
                        f11 = v[10];
                        f12 = v[11];
                        f13 = v[12];
                   */
                        double[] faceresult = face_data.Predict(face_recognition_model);
                        facename = Convert.ToInt16(faceresult[0]);
                      //  facename =facemodel.Labels[maxconfidenceindex];
                        faceshow++;
                    }

                    int labelnum = face_recognition_model.Labels[maxconfidenceindex];
                  
                    if (threshold > maxconfidence)
                    {
                       // Console.WriteLine("Unknow");
                        facename = 0;
                        display.Content = "Unknow";
                        facefail++;
                    }
                   
           
            
            return facename;
       
        }

        // Is multi_task, Color stream, depth stream and skeleton stream, i have use skeleton and color steam only, but when we use skeleton it needs infrad ray(depth)
        public void AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {

            DateTime timestamp;
            timestamp = DateTime.Now;
            Coordinate velocity = new Coordinate();
            Coordinate acceleration = new Coordinate();
            Coordinate CM = new Coordinate();
            Coordinate GRF = new Coordinate();
            CM = calculation.center_mass(pointAnkleLeft, pointAnkleRight, pointFootLeft, pointFootRight, pointHand, pointHand2, pointHipCenter);
            acceleration = calculation.getAcceleration(prevCM, earlyCM, CM, timestampPrev, timestampEarly, timestamp);
            velocity = calculation.getVelocity(CM, timestamp, prevCM, timestampPrev);
            prev_velCM = velocity;
            timestampEarly = timestampPrev;
            timestampPrev = timestamp;
            earlyCM = prevCM;
            prevCM = CM;
            
            SkeletonFrame skeletonframe = e.OpenSkeletonFrame();
            ColorImageFrame colorFrame = e.OpenColorImageFrame();
        
            if (colorFrame == null) return;
            if (skeletonframe == null) return;


            if (colorFrame != null)
            {
                // Copy the pixel data from the image to a temporary array
                colorFrame.CopyPixelDataTo(colorPixels);

                // Write the pixel data into our bitmap
                this.colorBitmap.WritePixels(
                    new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight),
                    this.colorPixels,
                    this.colorBitmap.PixelWidth * sizeof(int),
                    0);

                // do the face_detection, and feature extraction also recognition
          face_detection(colorFrame,colorPixels);
            
            
            }
       //  &&facelabel=="aa"
          //  the facelabel, when the facial part is done then return the label, process the skeletonframe
            if (skeletonframe != null &&facelabel=="aa")
                {
                   
                    skeletons = new Skeleton[skeletonframe.SkeletonArrayLength];
                    skeletonframe.CopySkeletonDataTo(skeletons);
                    foreach (Skeleton skeleton in skeletons)
                    {
                        if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                        {
                           // timertest.Start();
                           // timertest.Tick += close;
                           
                            if (skeleton != null)
                            {                
                               // write out the skeleton information as numerical calculating
                                                                                                             //Name_Height_Class_Times
                              //  FileStream fs = new FileStream(@"C:\Users\temp\Desktop\Activity training_dataset\terry_58_7_3.txt", FileMode.Append, FileAccess.Write);
                              //  StreamWriter sw = new StreamWriter(fs);

                                FileStream fs1 = new FileStream(@"Dataset\ADLfall_test1.txt", FileMode.Create, FileAccess.Write);
                                StreamWriter sw1 = new StreamWriter(fs1);

                                if (PostureDetector.GetDistanceFromFloor1(e, skeleton.Joints[JointType.Spine]) != -1 && PostureDetector.GetDistanceFromFloor1(e, skeleton.Joints[JointType.Head]) != -1)
                                {
                                    if (Double.IsNaN(velocity.X) == false && Double.IsNaN(acceleration.magnitude()) == false)
                                    {
                                        if (Double.IsInfinity(velocity.X) == false && Double.IsInfinity(acceleration.magnitude()) == false)
                                        {
                                            try
                                            {
                                                // the feature we selected, which still need to find the best combination, but the model i build which has 85% accuracy now.
                                           //    sw.WriteLine("7 " + "1:" + velocity.X + " 2:" + PostureDetector.GetDistanceFromFloor1(e, skeleton.Joints[JointType.Head]) + " 3:" + acceleration.magnitude() + " 4:" + PostureDetector.GetDistanceFromFloor1(e, skeleton.Joints[JointType.Spine]) + " 5:" + PostureDetector.GetDistanceFromFloor1(e, skeleton.Joints[JointType.HipCenter]) + " 6:" + PostureDetector.sitcon(e, skeleton.Joints[JointType.HipLeft], skeleton.Joints[JointType.KneeLeft]) + " 7:" + PostureDetector.sitcon(e, skeleton.Joints[JointType.HipRight], skeleton.Joints[JointType.KneeRight]) + " 8:" + PostureDetector.sitcon(e, skeleton.Joints[JointType.AnkleRight], skeleton.Joints[JointType.KneeRight]) + " 9:" + PostureDetector.sitcon(e, skeleton.Joints[JointType.AnkleLeft], skeleton.Joints[JointType.KneeLeft]) + " 10:" + (PostureDetector.GetDistanceFromFloor1(e, skeleton.Joints[JointType.Head]))/(PostureDetector.GetDistanceFromFloor1(e, skeleton.Joints[JointType.HipCenter])));
                                                sw1.WriteLine("1 " + "1:" + velocity.X + " 2:" + PostureDetector.GetDistanceFromFloor1(e, skeleton.Joints[JointType.Head]) + " 3:" + acceleration.magnitude() + " 4:" + PostureDetector.GetDistanceFromFloor1(e, skeleton.Joints[JointType.HipCenter]));

                                            }

                                            catch
                                            {
                                            }
                                        }
                                    }
                                }

                             //   sw.Close();
                                sw1.Close();

                                // start the classification
                               SVM_Classification();

                            }
                         
                        }
               
                    }
             
                    using (DrawingContext dc = this.drawingGroup.Open())
                    {
                        
                       dc.DrawRectangle(Brushes.Transparent, null, new Rect(0.0, 0.0, RenderWidth, RenderHeight));
                        if (skeletons.Length != 0)
                        {
                            foreach (Skeleton skel in skeletons)
                            {
                                if (skel.TrackingState == SkeletonTrackingState.Tracked)
                                {
                                    //it has official API is tell the different Skeleton or track specific skeleton
                                    trackingID = skel.TrackingId;
                                    sensor.SkeletonStream.AppChoosesSkeletons = true;
                                    sensor.SkeletonStream.ChooseSkeletons(trackingID);
                                    DrawBonesAndJoints(skel, dc);
                                   

                                    if (first) 
                                    { 
                                //      Console.WriteLine("Tracking ID :" + trackingID+"  You are :" + facelabel);
                                        displayforske.Content="Tracking ID :" + trackingID + "  You are :" + facelabel;
                                        first = false;
                                    }
                                }
                               
                              /*     
                                else if (skel.TrackingState == SkeletonTrackingState.PositionOnly)
                                {
                                    dc.DrawEllipse(
                                    centerPointBrush,
                                    null,
                                    SkeletonPointToScreen(skel.Position),
                                    BodyCenterThickness,
                                    BodyCenterThickness);
                                }
                                */
                            }
                        }
                    }
                }

              
        }

       
        SkeletonPoint pointShoulder = new SkeletonPoint();
        SkeletonPoint pointElbow = new SkeletonPoint();
        SkeletonPoint pointWrist = new SkeletonPoint();
        SkeletonPoint pointHand = new SkeletonPoint();
        SkeletonPoint pointShoulder2 = new SkeletonPoint();
        SkeletonPoint pointElbow2 = new SkeletonPoint();
        SkeletonPoint pointWrist2 = new SkeletonPoint();
        SkeletonPoint pointHand2 = new SkeletonPoint();
        SkeletonPoint pointHipCenter = new SkeletonPoint();
        SkeletonPoint pointHipLeft = new SkeletonPoint();
        SkeletonPoint pointHipRight = new SkeletonPoint();
        SkeletonPoint pointAnkleLeft = new SkeletonPoint();
        SkeletonPoint pointAnkleRight = new SkeletonPoint();
        SkeletonPoint pointFootLeft = new SkeletonPoint();
        SkeletonPoint pointFootRight = new SkeletonPoint();
        // Draws a skeleton's bones and joints
    
 
      
        public void DrawBonesAndJoints(Skeleton skeleton, DrawingContext drawingContext)
        {
    
          //  DrawBone(skeleton, drawingContext, JointType.Head, JointType.HipRight);
            // Render Torso
      /*
            DrawBone(skeleton, drawingContext, JointType.Head, JointType.ShoulderCenter);
            DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderLeft);
            DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.ShoulderRight);
            DrawBone(skeleton, drawingContext, JointType.ShoulderCenter, JointType.Spine);
            DrawBone(skeleton, drawingContext, JointType.Spine, JointType.HipCenter);
            DrawBone(skeleton, drawingContext, JointType.HipCenter, JointType.HipLeft);
            DrawBone(skeleton, drawingContext, JointType.HipCenter, JointType.HipRight);
       */
  

            // Render Joints
            foreach (Joint joint in skeleton.Joints)
            {
                Brush drawBrush = null;
                JointType jointType = joint.JointType;
                if (joint.TrackingState == JointTrackingState.Tracked)
                { 
                    drawBrush = trackedJointBrush;
                    SkeletonPoint jointPosition = joint.Position;
                    if (joint.TrackingState == JointTrackingState.Tracked)
                    {
                        drawBrush = this.trackedJointBrush;
                       
                       
                        if (jointType.Equals(JointType.ShoulderRight))
                        {
                            pointShoulder = jointPosition;
                        }
                        else if (jointType.Equals(JointType.ElbowRight))
                        {
                            pointElbow = jointPosition;
                        }
                        else if (jointType.Equals(JointType.WristRight))
                        {
                            pointWrist = jointPosition;
                        }
                        else if (jointType.Equals(JointType.HandRight))
                        {
                            pointHand = jointPosition;
                        }
                        if (jointType.Equals(JointType.ShoulderLeft))
                        {
                            pointShoulder2 = jointPosition;
                        }
                        else if (jointType.Equals(JointType.ElbowLeft))
                        {
                            pointElbow2 = jointPosition;
                        }
                        else if (jointType.Equals(JointType.WristLeft))
                        {
                            pointWrist2 = jointPosition;
                        }
                        else if (jointType.Equals(JointType.HandLeft))
                        {
                            pointHand2 = jointPosition;
                        }

                        else if (jointType.Equals(JointType.HipCenter))
                        {
                            pointHipCenter = jointPosition;
                        }
                        else if (jointType.Equals(JointType.HipLeft))
                        {
                            pointHipLeft = jointPosition;
                        }
                        else if (jointType.Equals(JointType.HipRight))
                        {
                            pointHipRight = jointPosition;
                        }
                     

                    }

                }
                else if (joint.TrackingState == JointTrackingState.Inferred)
                {
                    drawBrush = inferredJointBrush; 
                }

                if (drawBrush != null)
                { drawingContext.DrawEllipse(drawBrush, null, SkeletonPointToScreen(joint.Position), JointThickness, JointThickness); }
                
               
            }
  
       
        }

        // Maps a SkeletonPoint to lie within our render space and converts to Point
        private Point SkeletonPointToScreen(SkeletonPoint skelpoint)
        {
            // Convert point to depth space.  
            DepthImagePoint depthPoint = this.sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skelpoint, DepthImageFormat.Resolution640x480Fps30);
            return new Point(depthPoint.X, depthPoint.Y);
            
        }

        // Draws a bone line between two joints
        private void DrawBone(Skeleton skeleton, DrawingContext drawingContext, JointType jointType0, JointType jointType1)
        {

            Joint joint0 = skeleton.Joints[jointType0];
            Joint joint1 = skeleton.Joints[jointType1];

            // If we can't find either of these joints, exit
            if (joint0.TrackingState == JointTrackingState.NotTracked ||
                joint1.TrackingState == JointTrackingState.NotTracked)
            { return; }

            // Don't draw if both points are inferred
            if (joint0.TrackingState == JointTrackingState.Inferred &&
                joint1.TrackingState == JointTrackingState.Inferred)
            { return; }

            // We assume all drawn bones are inferred unless BOTH joints are tracked
        
            Pen drawPen = this.inferredBonePen;
            if (joint0.TrackingState == JointTrackingState.Tracked && joint1.TrackingState == JointTrackingState.Tracked)
            { drawPen = this.trackedBonePen; }
          
            drawingContext.DrawLine(drawPen, SkeletonPointToScreen(joint0.Position), SkeletonPointToScreen(joint1.Position));
       //     drawingContext.DrawEllipse(Brushes.Black, new Pen(Brushes.Black, 1), new Point(Width / 2, Height / 2), Width / 2, Height / 2);
            
 
        }

        /*
        int i = 0;
        private void saveImage(object sender, EventArgs e)
        {
             
            string path = "C:\\Users\\temp\\Desktop\\Fall.jpg";
            FileStream fs = new FileStream(path, FileMode.Create);
            RenderTargetBitmap bmp = new RenderTargetBitmap((int)ColorImage.ActualWidth,
                (int)ColorImage.ActualHeight, 1 / 96, 1 / 96, PixelFormats.Pbgra32);
            bmp.Render(ColorImage);
            BitmapEncoder encoder = new JpegBitmapEncoder();//new TiffBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bmp));
            encoder.Save(fs);
            fs.Close();
            i++;
        }
     
         * */
        int facecount = 0;
   
     //   string facepath = @"C:\Users\temp\Desktop\Face_1011\01.jpg";
        private void facecapture_Click(object sender, RoutedEventArgs e)
        {

            if (facelist.Count != 0)
            {
             //   string path = @"C:\Users\temp\Desktop\1011_Face\0" + facecount + ".jpg";

                facecount = facecount % 11;
                facelist.Last().Bitmap.Save(@"C:\Users\temp\Desktop\1011_Face\test0" + facecount + frontfaceposing[facecount] + ".jpg");
                ++facecount;
                                if (facecount == 11) { facecount = 0; }
                                
                                Instructuring_Training.Content = "Please Posing " + frontfaceposing[facecount];
                                Instructuring_Training.FontSize = 14;
                                Instructuring_Training.FontStyle = FontStyles.Normal;
                                Instructuring_Training.Foreground = Brushes.Red;
                                Instructuring_Training.Background = Brushes.Black;
                                Console.WriteLine("You have stored!");
                          
                         //   capturedface_training = facedetection.extract1_feature(facelist.Last());
                           
                         //   face_training(capturedface_training);
    
            }
          
        }
     
        private void close(object sender, EventArgs e)
        {
          //  main.Close();
            Application.Current.Shutdown();
           
        }
     

    
     
    }
}
