using LibSVMsharp.Helpers;
using LibSVMsharp.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibSVMsharp.Examples.Classification
{
    class Program
    {
        static void Main(string[] args)
        {
            // Load the datasets: In this example I use the same datasets for training and testing which is not suggested
            SVMProblem trainingSet = SVMProblemHelper.Load(@"C:\Users\temp\Desktop\ADLfall_train.txt");
        //    SVMProblem testSet = SVMProblemHelper.Load(@"C:\Users\temp\Desktop\ADLfall_test.txt");
            SVMProblem testSet1 = SVMProblemHelper.Load(@"C:\Users\temp\Desktop\ADLfall_test1.txt");
           // SVMProblem testSet1 = SVMProblemHelper.Load(@"C:\Users\temp\Desktop\result.txt");

            // Normalize the datasets if you want: L2 Norm => x / ||x||
           trainingSet = trainingSet.Normalize(SVMNormType.L2);
        //   testSet = testSet.Normalize(SVMNormType.L2);
          testSet1 = testSet1.Normalize(SVMNormType.L2);
            // Select the parameter set
         
            SVMParameter parameter = new SVMParameter();

            parameter.Type = SVMType.C_SVC;
            parameter.Kernel = SVMKernelType.RBF;
            parameter.C = 32768.0;
            parameter.Gamma = 8.0;
          

            // Do cross validation to check this parameter set is correct for the dataset or not
            double[] crossValidationResults; // output labels
            int nFold = 5;
       //  trainingSet1.CrossValidation(parameter, nFold, out crossValidationResults);
    
            // Evaluate the cross validation result
            // If it is not good enough, select the parameter set again
        //  double crossValidationAccuracy = trainingSet.EvaluateClassificationProblem(crossValidationResults);

            // Train the model, If your parameter set gives good result on cross validation
   //   SVMModel model = trainingSet.Train(parameter);
           
      
            // Save the model
     //   SVM.SaveModel(model, @"Model\activity_recognition.txt");
           SVMModel model = SVM.LoadModel(@"Model\activity_recognition.txt");
          
            int p,q,w,e,r,ok=0;
            double sum;
            q = 0;
            w = 0;
            e = 0;
            r = 0;
            // Predict the instances in the test set
            double[] testResults = testSet1.Predict(model);

            while(ok<testSet1.Length)
            {
           
              var resut = model.Predict(testSet1.X[ok]);
          //    Console.WriteLine("resut111:" + resut);
                p = Convert.ToInt16(resut);
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

                ok++;
            }
            sum = q + w + e + r;


            Console.WriteLine("result:" + Math.Round(q / sum, 2) + "," + Math.Round(w / sum, 2) + "," + Math.Round(e/ sum, 2) + "," + Math.Round(r / sum, 2));
            // Evaluate the test results
          
            int[,] confusionMatrix;
            double testAccuracy = testSet1.EvaluateClassificationProblem(testResults, model.Labels, out confusionMatrix);

            // Print the resutls
     //  Console.WriteLine("\n\nCross validation accuracy: " + crossValidationAccuracy);
           Console.WriteLine("\nTest accuracy: " + testAccuracy);
            Console.WriteLine("\nConfusion matrix:\n");

            // Print formatted confusion matrix
            Console.Write(String.Format("{0,6}", ""));
            for (int i = 0; i < model.Labels.Length; i++)
                Console.Write(String.Format("{0,5}", "(" + model.Labels[i] + ")"));
            Console.WriteLine();
            for (int i = 0; i < confusionMatrix.GetLength(0); i++)
            {
                Console.Write(String.Format("{0,5}", "(" + model.Labels[i] + ")"));
                for (int j = 0; j < confusionMatrix.GetLength(1); j++)
                    Console.Write(String.Format("{0,5}", confusionMatrix[i,j]));
                Console.WriteLine();
            }

            Console.WriteLine("\n\nPress any key to quit...");
            Console.ReadLine();
        }
    }
}
