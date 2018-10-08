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

namespace WpfApplication1
{
    class Face_Section
    {

        int index = 12;

        public void face_training(SVMProblem f_training)
        {

            SVMProblem trainingSet = SVMProblemHelper.Load(@"C:\Users\temp\Desktop\0921_towp.txt");
            SVMProblem testSet = SVMProblemHelper.Load(@"C:\Users\temp\Desktop\0921_towpt.txt");
            // f_training.Save(@"C:\Users\temp\Desktop\1005f.txt");
            //  trainingSet.Insert(index, f_training.X[0], 2);
            trainingSet.Add(f_training.X[0], 1);
            trainingSet.Save(@"C:\Users\temp\Desktop\flag.txt");
            //   trainingSet.Save(@"C:\Users\temp\Desktop\1005.txt");
            // Console.WriteLine();
            //   SVMNode node = new SVMNode();
            //  node.Index = Convert.ToInt32(o);
            //  node.Value = Convert.ToDouble(f_training.X);
            //  nodes.Add(node);
            //  trainingSet.Add(nodes.ToArray(), 1);
            //  int number = randon.Next(0, trainingSet.X.Count);
            //  int trainingsample = Convert.ToInt32(trainingSet.X.Count * 2 / 3);
            //  int testingsample = Convert.ToInt32(trainingSet.X.Count / 3);

            trainingSet = trainingSet.Normalize(SVMNormType.L2);
            testSet = testSet.Normalize(SVMNormType.L2);

            SVMParameter parameter = new SVMParameter();
            parameter.Type = SVMType.NU_SVC;
            parameter.Kernel = SVMKernelType.SIGMOID;
            parameter.C = 1;
            parameter.Gamma = 1;
            parameter.Probability = true;
            int nFold = 10;
            MainWindow main = new MainWindow();
            double[] crossValidationResults; // output labels
            trainingSet.CrossValidation(parameter, nFold, out crossValidationResults);
            double crossValidationAccuracy = trainingSet.EvaluateClassificationProblem(crossValidationResults);
            SVMModel model = SVM.Train(trainingSet, parameter);
            // SVMModel model = trainingSet.Train(parameter);
           
            SVM.SaveModel(model, @"C:\Users\temp\Desktop\1005.txt");

            double[] testResults = testSet.Predict(model);
            //     Console.WriteLine("");
            int[,] confusionMatrix;
            double testAccuracy = testSet.EvaluateClassificationProblem(testResults, model.Labels, out confusionMatrix);
            // Console.WriteLine("\n\nCross validation accuracy: " + crossValidationAccuracy);
            //  Console.WriteLine("testAccuracy:" + testAccuracy);
            //  Console.WriteLine(Convert.ToString(trainingSet.X.Count));
            main.Training_result.Content = "testAccuracy:" + testAccuracy + "\nCross validation accuracy: " + crossValidationAccuracy + "\nCount " + trainingSet.X.Count;
            main.Training_result.FontSize = 14;
            main.Training_result.FontStyle = FontStyles.Normal;
            main.Training_result.Foreground = Brushes.Red;
            main.Training_result.Background = Brushes.Black;
            // Console.WriteLine(trainingSet1.Length);
            //  trainingSet.Save(@"C:\Users\temp\Desktop\1005.txt");
            index++;
        }
    }
}
