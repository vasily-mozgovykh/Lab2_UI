using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Legends;
using OxyPlot.Axes;

namespace MainUsefInterface
{
    public class OxyPlotModel
    {
        public OxyPlotData Data;
        public PlotModel PlotModel { get; private set; }

        public OxyPlotModel(OxyPlotData data)
        {
            Data = data;
            PlotModel = new PlotModel { Title = "" };
            AddSeries();
        }

        public void AddSeries()
        {
            PlotModel.Series.Clear();

            OxyColor[] colors = new OxyColor[2] { OxyColors.Red, OxyColors.MidnightBlue };

            ScatterSeries scatterSeries = new ScatterSeries();
            for (int i = 0; i < Data.XL[0].Length; i++)
                scatterSeries.Points.Add(new ScatterPoint(Data.XL[0][i], Data.YL[0][i]));
            scatterSeries.MarkerType = MarkerType.Diamond;
            scatterSeries.MarkerSize = 4;
            scatterSeries.MarkerStroke = OxyColors.DarkCyan;
            scatterSeries.MarkerFill = colors[0];
            scatterSeries.Title = Data.Legends[0];

            LineSeries lineSeries = new LineSeries();
            for (int i = 0; i < Data.XL[1].Length; i++)
                lineSeries.Points.Add(new DataPoint(Data.XL[1][i], Data.YL[1][i]));
            lineSeries.Color = colors[1];
            lineSeries.MarkerType = MarkerType.Circle;
            lineSeries.MarkerSize = 3;
            lineSeries.MarkerStroke = colors[1];
            lineSeries.MarkerFill = colors[1];
            lineSeries.MarkerStrokeThickness = 1;
            lineSeries.Title = Data.Legends[1];

            Legend legend = new Legend { LegendPosition = LegendPosition.TopLeft };
            PlotModel.Legends.Add(legend);

            PlotModel.Series.Add(lineSeries);
            PlotModel.Series.Add(scatterSeries);
            double eps;

            double left = Data.XL[0][0];
            double right = Data.XL[0][Data.XL[0].Length - 1];
            eps = (right - left) / 20d;
            PlotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Minimum = left- eps,
                Maximum = right + eps,
                StringFormat = "0.000"
            });

            double min, max;
            double top0 = Data.YL[0].Max(), top1 = Data.YL[1].Max();
            double bottom0 = Data.YL[0].Min(), bottom1 = Data.YL[1].Min();
            max = top0 < top1 ? top1 : top0;
            min = bottom0 < bottom1 ? bottom0 : bottom1;
            eps = (max - min) / 20d;
            PlotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Minimum = min - eps, 
                Maximum = max + eps,
                StringFormat = "0.000"
            });
        }
    }
}
