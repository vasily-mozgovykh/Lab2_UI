using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using MainUsefInterface;
using OxyPlot;
using SplinesDataStructures;

namespace MainUserInterface
{
    public class ViewData : INotifyPropertyChanged, IDataErrorInfo
    {
        #region FIELDS
        /* RawData */
        private double[] LimitsValue { get; set; }
        private int NodesCountValue { get; set; }

        private bool IsUniformValue { get; set; }
        private FRawEnum FunctionNameValue { get; set; }

        /* SplineData */
        public int SplineNodesCount { get; set; }
        public double LeftFirstDerivative { get; set; }
        public double RightFirstDerivative { get; set; }

        /* Refs */
        private RawData rawData { get; set; }
        private SplineData splineData { get; set; }
        public List<SplineDataItem> Items { get; set; }
        #endregion

        #region PUBLIC PROPERTIES
        public double[] Limits
        {
            get => LimitsValue;
            set
            {
                if (value != null && (value[0] != LimitsValue[0] || value[1] != LimitsValue[1]))
                {
                    LimitsValue[0] = value[0]; LimitsValue[1] = value[1];
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Limits)));
                }
            }
        }
        public int NodesCount
        {
            get => NodesCountValue;
            set
            {
                if (value != NodesCountValue)
                {
                    NodesCountValue = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NodesCount)));
                }
            }
        }
        public bool IsUniform
        {
            get => IsUniformValue;
            set
            {
                if (value != IsUniformValue)
                {
                    IsUniformValue = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsUniform)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsNonUniform)));
                }
            }
        }
        public bool IsNonUniform
        {
            get => !IsUniformValue;
            set
            {
                if (value != !IsUniformValue)
                {
                    IsUniformValue = !value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsUniform)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsNonUniform)));
                }
            }
        }
        public FRawEnum FunctionName
        {
            get => FunctionNameValue;
            set
            {
                if (value != FunctionNameValue)
                {
                    FunctionNameValue = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FunctionName)));
                }
            }
        }

        public bool IsLoaded { get; set; } = false;

        public OxyPlotModel OxyPlotModel { get; set; }
        #endregion

        public ViewData()
        {
            LimitsValue = new double[2] { -1d, 1d };
            NodesCountValue = 3;
            IsUniformValue = true;
            FunctionNameValue = FRawEnum.Linear;
            rawData = new RawData(Limits[0], Limits[1], NodesCountValue, IsUniform, RawData.Linear);

            SplineNodesCount = 5;
            LeftFirstDerivative = 3;
            RightFirstDerivative = 3;
            splineData = new SplineData(rawData, new double[2] { LeftFirstDerivative, RightFirstDerivative }, SplineNodesCount);

            Items = splineData.Items;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        #region INPUT/OUTPUT
        public void Save(string filename)
        {
            rawData.Save(filename);
            //RawDataFromControls();
        }

        public void Load(string filename)
        {
            IsLoaded = false;
            try
            {
                rawData = RawData.Load(filename);
                Limits = new double[2] { rawData.Left, rawData.Right };
                NodesCount = rawData.NodesCount;
                IsUniform = rawData.IsUniform;
                IsNonUniform = !(rawData.IsUniform);
                switch (rawData.Function.Method.Name)
                {
                    case "Linear":
                        FunctionName = FRawEnum.Linear; break;
                    case "Cubic":
                        FunctionName = FRawEnum.Cubic; break;
                    case "Cosine":
                        FunctionName = FRawEnum.Cosine; break;
                    default:
                        FunctionName = FRawEnum.PseudoRandom; break;
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            splineData.RawData = rawData;
            splineData.SplineNodesCount = SplineNodesCount;
            splineData.BoundaryConditions[0] = LeftFirstDerivative;
            splineData.BoundaryConditions[1] = RightFirstDerivative;
            IsLoaded = true;
        }

        public void RawDataFromControls()
        {
            IsLoaded = false;
            FRaw function = RawData.Linear;
            switch (FunctionName)
            {
                case FRawEnum.Linear:
                    function = RawData.Linear; break;
                case FRawEnum.Cubic:
                    function = RawData.Cubic; break;
                case FRawEnum.Cosine:
                    function = RawData.Cosine; break;
                case FRawEnum.PseudoRandom:
                    function = RawData.PseudoRandom; break;
            }
            rawData = new RawData(Limits[0], Limits[1], NodesCountValue, IsUniform, function);

            splineData.RawData = rawData;
            splineData.SplineNodesCount = SplineNodesCount;
            splineData.BoundaryConditions[0] = LeftFirstDerivative;
            splineData.BoundaryConditions[1] = RightFirstDerivative;
        }
        #endregion

        public void CalculateSpline()
        {
            splineData.CalculateSpline();
        }

        public double GetIntegral() => splineData.Integral;

        public record NodeValue(double Node, double Value);

        public NodeValue GetNodeValue(int index) => new NodeValue(rawData.Nodes[index], rawData.Values[index]);

        public void RenderPlot()
        {
            OxyPlotData data = new OxyPlotData();
            data.AddPoints(rawData, Items);

            OxyPlotModel = new OxyPlotModel(data);
            OxyPlotModel.AddSeries();
        }

        #region VALIDATION
        public string Error { get { return "Error message"; } }

        public string this[string property]
        {
            get
            {
                string msg = null;
                switch (property)
                {
                    case "NodesCount":
                        if (NodesCount < 2)
                            msg = "Nodes count should be strictly greather than 2";
                        break;
                    case "SplineNodesCount":
                        if (SplineNodesCount < 2)
                            msg = "Spline nodes count should be strictly greater than 2";
                        break;
                    case "Limits":
                        if (Limits[0] >= Limits[1])
                            msg = "Left limit should be strictly less than right limit";
                        break;
                    default:
                        break;
                }
                return msg;
            }
        }
        #endregion
    }
}
