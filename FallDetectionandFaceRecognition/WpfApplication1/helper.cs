using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibSVMsharp;
namespace WpfApplication1
{
    class helper
    {
        public static bool Save(SVMProblem problem, string filename)
        {
            if (String.IsNullOrWhiteSpace(filename) || problem == null || problem.Length == 0)
            {
                return false;
            }
            
            //    //  StreamWriter sw = new StreamWriter(fs);
            NumberFormatInfo provider = new NumberFormatInfo();
            provider.NumberDecimalSeparator = ".";
           
            using (StreamWriter sw = new StreamWriter(filename))
            {
                for (int i = 0; i < problem.Length; i++)
                {
                   // sw.Write("\r\n");
                  
                    sw.Write(problem.Y[i]);

                    if (problem.X[i].Length > 0)
                    {
                        sw.Write(" ");

                        for (int j = 0; j < problem.X[i].Length; j++)
                        {
                            sw.Write(problem.X[i][j].Index);
                            sw.Write(":");
                            sw.Write(problem.X[i][j].Value.ToString(provider));

                            if (j < problem.X[i].Length - 1)
                            {
                                sw.Write(" ");
                            }
                        }
                    }

                    sw.Write("\n");
                }
            }

            return true;
        }
    }
}
