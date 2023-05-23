using System.Text.Json;
using System.Runtime.InteropServices;

namespace SplinesDataStructures
{
    public delegate double FRaw(double x);

    public enum FRawEnum
    {
        Linear,
        Cubic,
        Cosine,
        PseudoRandom
    }

    public class RawData
    {
        #region FIELDS
        public double Left { get; set; }
        public double Right { get; set; }
        public int NodesCount { get; set; }
        public bool IsUniform { get; set; }
        public FRaw Function { get; set; }
        public double[] Nodes { get; set; }
        public double[] Values { get; set; }
        #endregion

        public RawData(double left, double right, int nodesCount, bool isUniform, FRaw function)
        {
            Left = left;
            Right = right;
            NodesCount = nodesCount;
            IsUniform = isUniform;
            Function = function;

            Nodes = new double[NodesCount];
            Values = new double[NodesCount];
            Random rand = new Random();
            double step = (right - left) / (NodesCount - 1);
            for (int i = 0; i < NodesCount; i++)
            {
                Nodes[i] = Left + step * i;
                if (i != 0 && i !=  NodesCount - 1 && !IsUniform)
                {
                    Nodes[i] += step * (-0.49d + 0.99d * rand.NextDouble());
                } 
                Values[i] = function(Nodes[i]);
            }
        }

        public RawData(string filename)
        {
            RawData rawData = Load(filename);

            Left = rawData.Left;
            Right = rawData.Right;
            NodesCount = rawData.NodesCount;
            Values = rawData.Values;
            Function = Cosine;

            Nodes = new double[NodesCount];
            Values = new double[NodesCount];
            rawData.Nodes.CopyTo(Nodes, 0);
            rawData.Values.CopyTo(Values, 0);
        }

        #region DEFAULT MATH FUNCTIONS
        public static double Linear(double x)
        {
            return x;
        }

        public static double Cubic(double x)
        {
            return Math.Pow(x, 3);
        }

        public static double Cosine(double x)
        {
            return Math.Cos(x);
        }

        public static double PseudoRandom(double x)
        {
            Random rand = new Random();
            return rand.NextDouble();
        }
        #endregion

        #region INPUT/OUTPUT
        public void Save(string filename)
        {
            JsonSerializerOptions options = new JsonSerializerOptions()
            {
                IncludeFields = true,
                WriteIndented = true
            };

            StreamWriter? writer = null;
            try
            {
                writer = new StreamWriter(filename);
                string jsonLeft = JsonSerializer.Serialize(Left, options);
                string jsonRight = JsonSerializer.Serialize(Right, options);
                string jsonNodesCount = JsonSerializer.Serialize(NodesCount, options);
                string jsonIsUniform = JsonSerializer.Serialize(IsUniform, options);
                string jsonFunctionName = Function.Method.Name;
                double[] values = new double[2 * NodesCount];
                for (int i = 0; i < NodesCount; i++)
                {
                    values[2 * i] = Nodes[i];
                    values[2 * i + 1] = Values[i];
                }
                string jsonValues = JsonSerializer.Serialize(values, options);
                writer.WriteLine(jsonLeft);
                writer.WriteLine(jsonRight);
                writer.WriteLine(jsonNodesCount);
                writer.WriteLine(jsonIsUniform);
                writer.WriteLine(jsonFunctionName);
                writer.WriteLine(jsonValues);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            finally
            {
                writer?.Close();
            }
        }

        public static RawData Load(string filename)
        {
            JsonSerializerOptions options = new JsonSerializerOptions()
            {
                IncludeFields = true,
                WriteIndented = true
            };

            StreamReader? reader = null;
            double left, right;
            int nodesCount;
            bool isUniform;
            FRaw? function = null;
            double[]? values = Array.Empty<double>();

            try
            {
                reader = new StreamReader(filename);
                string? jsonLeft = reader.ReadLine();
                string? jsonRight = reader.ReadLine();
                string? jsonNodesCount = reader.ReadLine();
                string? jsonIsUniform = reader.ReadLine();
                string? jsonFunctionName = reader.ReadLine();
                string? jsonValues = reader.ReadToEnd();

                if (jsonLeft == null || jsonRight == null || jsonNodesCount == null || jsonIsUniform == null || jsonFunctionName == null)
                {
                    throw new Exception($"Incorrect input in file {filename}");
                }
                left = JsonSerializer.Deserialize<double>(jsonLeft, options);
                right = JsonSerializer.Deserialize<double>(jsonRight, options);
                nodesCount = JsonSerializer.Deserialize<int>(jsonNodesCount, options);
                isUniform = JsonSerializer.Deserialize<bool>(jsonIsUniform, options);

                FRaw func = Linear;
                switch (jsonFunctionName)
                {
                    case "Linear":
                        function = Linear; break;
                    case "Cubic":
                        function = Cubic; break;
                    case "Cosine":
                        function = Cosine; break;
                    default:
                        function = PseudoRandom; break;
                }

                values = JsonSerializer.Deserialize<double[]>(jsonValues, options);
                if (values == null || values.Length != 2 * nodesCount)
                    throw new Exception("Can't load RawData values");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            finally
            {
                reader?.Close();
            }
            RawData newData = new RawData(left, right, nodesCount, isUniform, function);
            for (int i = 0; i < nodesCount; i++)
            {
                newData.Nodes[i] = values[2 * i];
                newData.Values[i] = values[2 * i + 1];
            }
            return newData;
        }
        #endregion
    }

    public struct SplineDataItem
    {
        #region FIELDS
        public double X { get; }
        public double Value { get; }
        public double FirstDerivative { get; }
        public double SecondDerivative { get; }
        #endregion

        public SplineDataItem(double x, double value, double firstDerivative, double secondDerivative)
        {
            X = x;
            Value = value;
            FirstDerivative = firstDerivative;
            SecondDerivative = secondDerivative;
        }

        public string ToString(string format)
        {
            return $"x = {X.ToString(format)}" +
                   $", S(x) = {Value.ToString(format)}" +
                   $", S'(x) = {FirstDerivative.ToString(format)}" +
                   $", S''(x) = {SecondDerivative.ToString(format)}";
        }

        public override string ToString()
        {
            return ToString("0.000");
        }
    }

    public class SplineData
    {
        #region FIELDS
        public RawData RawData { get; set; }
        public double[] BoundaryConditions { get; set; }
        public int SplineNodesCount { get; set; }
        public List<SplineDataItem> Items { get; set; }
        public double Integral { get; set; }
        #endregion

        public SplineData(RawData rawData, double[] boundaryConditions, int splineNodesCount)
        {
            RawData = rawData;
            BoundaryConditions = new double[2];
            boundaryConditions.CopyTo(BoundaryConditions, 0);
            SplineNodesCount = splineNodesCount;
            Items = new List<SplineDataItem>();
        }

        #region DLL INFO        
        private enum SPL_ERROR
        {
            SPL_SUCCESS = 0,
            SPL_NEW_TASK = 1,
            SPL_MEM_ALLOC = 2,
            SPL_EDIT_SPLINE = 3,
            SPL_CONSTRUCT = 4,
            SPL_INTERPOLATE = 5,
            SPL_INTEGRATE = 6,
            SPL_DELETE_TASK = 7,
            SPL_UNKNOWN = 127
        };

        private Dictionary<SPL_ERROR, string> ErrorMessages = new Dictionary<SPL_ERROR, string>() {
            { SPL_ERROR.SPL_SUCCESS, "All tasks performed successfully" },
            { SPL_ERROR.SPL_NEW_TASK, "Can't create new interpolation task" },
            { SPL_ERROR.SPL_MEM_ALLOC, "Can't allocate enough memory for spline coefficients" },
            { SPL_ERROR.SPL_EDIT_SPLINE, "Can't configure spline parameters" },
            { SPL_ERROR.SPL_CONSTRUCT, "Can't construct a natural cubic spline" },
            { SPL_ERROR.SPL_INTERPOLATE, "Can't interpolate values" },
            { SPL_ERROR.SPL_INTEGRATE, "Can't integrate spline" },
            { SPL_ERROR.SPL_DELETE_TASK, "Error deleting interpolation task" },
            { SPL_ERROR.SPL_UNKNOWN, "Got an unknown error" }
        };

        private const string SplinesCalculatorDllPath = "C:\\Users\\Vasily\\source\\repos\\Lab2\\x64\\Debug\\SplinesCalculator.dll";

        [DllImport(SplinesCalculatorDllPath, CallingConvention = CallingConvention.Cdecl)]
        private static extern void calculate_spline(
            double[] nodes,
            int nodes_count,
            bool is_uniform,
            double[] values,
            double[] boundary_conditions,
            int nsite,
            double[] site,
            double left_integration_limit,
            double right_integration_limit,
            double[] spline_values,
            ref double integral,
            ref int error
        );
        #endregion

        public void CalculateSpline()
        {
            Items.Clear();

            double integral = 0d;
            int error = 0;

            double[] splineValues = new double[3 * SplineNodesCount];

            calculate_spline(
                RawData.IsUniform ? new double[2] { RawData.Left, RawData.Right } : RawData.Nodes,
                RawData.NodesCount,
                RawData.IsUniform,
                RawData.Values,
                BoundaryConditions,
                SplineNodesCount,
                new double[2] { RawData.Left, RawData.Right },
                RawData.Left,
                RawData.Right,
                splineValues,
                ref integral,
                ref error
            );

            if (error != 0)
            {
                SPL_ERROR error_code = error > 0 && error < 8 ? (SPL_ERROR)error : SPL_ERROR.SPL_UNKNOWN;
                throw new Exception(ErrorMessages[error_code] + $". Error code: {error}");
            }

            Integral = integral;

            double step = (RawData.Right - RawData.Left) / (SplineNodesCount - 1);
            for (int i = 0; i < SplineNodesCount; i++)
            {
                double x = RawData.Left + step * i;
                Items.Add(new SplineDataItem(x, splineValues[3 * i], splineValues[3 * i + 1], splineValues[3 * i + 2]));
            }
        }
    }
}