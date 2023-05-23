using SplinesDataStructures;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MainUsefInterface
{
    public class OxyPlotData
    {
        public List<double[]> XL { get; set; }
        public List<double[]> YL { get; set; } 

        public List<string> Legends { get; set; }

        public OxyPlotData()
        {
            Legends = new List<string>();
            XL = new List<double[]>();
            YL = new List<double[]>();
        }

        public void AddPoints(RawData rawData, List<SplineDataItem> items)
        {
            string functionName = rawData.Function.Method.Name;
            double[] X = new double[rawData.NodesCount];
            double[] Y = new double[rawData.NodesCount];
            for (int i = 0; i < rawData.NodesCount; i++)
            {
                X[i] = rawData.Nodes[i];
                Y[i] = rawData.Values[i];
            }
            XL.Add(X);
            YL.Add(Y);
            Legends.Add($"{functionName} f(x)");

            X = new double[items.Count];
            Y = new double[items.Count];
            for (int i = 0; i < items.Count; i++)
            {
                X[i] = items[i].X;
                Y[i] = items[i].Value;
            }
            XL.Add(X);
            YL.Add(Y);
            Legends.Add($"Spline S(x)");
        }
    }
}
