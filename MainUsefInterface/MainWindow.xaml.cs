using Microsoft.Win32;
using SplinesDataStructures;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MainUserInterface
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ViewData viewData = new ViewData();

        public MainWindow()
        {
            InitializeComponent();

            DataContext = viewData;
            functionSelectorComboBox.ItemsSource = Enum.GetValues(typeof(FRawEnum));
        }

        private void SaveRawData(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            try
            {
                if (dialog.ShowDialog() == true)
                {
                    viewData.Save(dialog.FileName);
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void LoadRawDataFromFile(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog dialog = new OpenFileDialog();
                if (dialog.ShowDialog() != true)
                    return;

                viewData.Load(dialog.FileName);
                CustomCommands.CalculateFromFileCommand.Execute("CalculateFromFile", null);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void CalculateRawDataFromControls(object sender, RoutedEventArgs e)
        {
            try
            {
                viewData.RawDataFromControls();
                viewData.CalculateSpline();

                FillRawDataNodesListBox();
                splineDataItemsListBox.Items.Refresh();

                integralTextBlock.Text = viewData.GetIntegral().ToString("0.000");
                viewData.RenderPlot();
                DataContext = viewData.OxyPlotModel;
                DataContext = viewData;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        private void FillRawDataNodesListBox()
        {
            rawDataNodesListBox.Items.Clear();

            for (int i = 0; i < viewData.NodesCount; i++)
            {
                var xf = viewData.GetNodeValue(i);
                string x = xf.Node.ToString("0.000");
                string f = xf.Value.ToString("0.000");
                rawDataNodesListBox.Items.Add($"f({x}) = {f}");
            }

            rawDataNodesListBox.Items.Refresh();
        }

        private void CanCalculateFromControlsCommandHandler(object sender, CanExecuteRoutedEventArgs e)
        {
            foreach (FrameworkElement element in gridMain.Children)
            {
                if (Validation.GetHasError(element) == true)
                {
                    e.CanExecute = false;
                    return;
                }
            }
            e.CanExecute = true;
        }

        private void CalculateFromControlsCommandHandler(object sender, ExecutedRoutedEventArgs e)
        {
            CalculateRawDataFromControls(sender, e);
        }

        private void CanCalculateFromFileCommandHandler(object sender, CanExecuteRoutedEventArgs e)
        {
            foreach (FrameworkElement element in gridMain.Children)
            {
                if (Validation.GetHasError(element) == true)
                {
                    e.CanExecute = false;
                    return;
                }
            }
            e.CanExecute = viewData.IsLoaded;
        }

        private void CalculateFromFileCommandHandler(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                viewData.CalculateSpline();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
                return;
            }

            FillRawDataNodesListBox();
            splineDataItemsListBox.Items.Refresh();

            integralTextBlock.Text = viewData.GetIntegral().ToString("0.000");
            viewData.RenderPlot();
            DataContext = viewData.OxyPlotModel;
            DataContext = viewData;
        }

        private void CanSaveCommandHandler(object sender, CanExecuteRoutedEventArgs e)
        {
            if (nodesCountTextBox == null || limitsTextBox == null)
            {
                e.CanExecute = false;
                return;
            }
            if (Validation.GetHasError(nodesCountTextBox) == true ||
                Validation.GetHasError(limitsTextBox) == true)
                e.CanExecute = false;
            else
                e.CanExecute = true;
        }

        private void SaveCommandHandler(object sender, ExecutedRoutedEventArgs e)
        {
            SaveRawData(sender, e);
        }
    }
}
