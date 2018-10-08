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
    class SVM_activity
    {
        int p, q, w, e, r;
        // the window i talked about last time which dealing the data in frame
        ConcurrentQueue<int> myq = new ConcurrentQueue<int>();
        SVMProblem testSet1 = new SVMProblem();
        SVMModel model = SVM.LoadModel(@"Model\main.activity_model.txt");
        public void SVM_Classification()
        {

            testSet1 = SVMProblemHelper.Load(@"Dataset\ADLfall_test1.txt");

            testSet1 = testSet1.Normalize(SVMNormType.L2);

            float sum;

            if (testSet1.Length != 0)
            {

                try
                {

                    //var resut = model.Predict(testSet1.X[testSet1.Length - 1]);
                    //  p = Convert.ToInt16(resut);
                    //predict the result using model, return result
                    var result = testSet1.Predict(model);
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
                if (myq.Count > 30)
                {

                    //dequeue the old one
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
                    MainWindow main = new MainWindow();
                    
                    //   main.activity.Content = ("Sit down:" + sit_down + "\n" + "Walking" + walkig + "\n" + "Standing" + standing + "\n" + "Fall event" + fallevent);
                  main.activity.Content = ("Sit down: " + Math.Round(e / sum, 2) * 100 + "%" + "\n" + "Walking: " + Math.Round(q / sum, 2) * 100 + "%" + "\n" + "Standing: " + Math.Round(w / sum, 2) * 100 + "%" + "\n" + "Fall event: " + Math.Round(r / sum, 2) * 100 + "%");
                    //  main.activity.Content = ("Sit down:" + Math.Round(h / sum, 2) + "\n" + "Walking" + Math.Round(w / sum, 2) + "\n" + "Standing" + Math.Round(q / sum, 2) + "\n" + "Fall event" + Math.Round(r / sum, 2));
                    if (e / sum > 0.5) { main.label.Content = ("You have sit down"); main.label.Foreground = Brushes.Red; }
                    else if (q / sum > 0.5) { main.label.Content = "You are walking"; main.label.Foreground = Brushes.Red; }
                    else if (w / sum > 0.5) { main.label.Content = "You are standing"; main.label.Foreground = Brushes.Red; }
                    else if (r / sum > 0.5) { main.label.Content = "You fell down"; main.label.Foreground = Brushes.Red; }

                    main.activity.FontSize = 20;
                    main.activity.FontStyle = FontStyles.Normal;
                    main.activity.Foreground = Brushes.Red;
                    main.activity.Background = Brushes.Black;
                    

                }

            }


        }
    }
}
