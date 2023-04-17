using System.Windows.Input;

namespace MainUserInterface
{
    public static class CustomCommands
    {
        public static RoutedCommand CalculateFromControlsCommand =
            new RoutedCommand("CalculateCommand", typeof(CustomCommands));

        public static RoutedCommand CalculateFromFileCommand =
            new RoutedCommand("CalculateCommand", typeof(CustomCommands));
    }
}
