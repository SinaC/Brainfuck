using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Piet
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            string filename = @"C:\Prj\Personal\Esolang\Piet\Programs\hello-medium.png";
            int codelSize = 4;
            //string filename = @"C:\Prj\Personal\Esolang\Piet\Programs\nprime.gif";
            //int codelSize = 8;

            NaiveInterpreter interpreter = new NaiveInterpreter();
            interpreter.Parse(filename, codelSize);
            interpreter.Execute();

            OriginalImage.Source = new BitmapImage(new Uri(filename, UriKind.Absolute));
            try
            {
                ParsedImage.Source = interpreter.GenerateImageFromCodels();
            }
            catch (Exception ex)
            {
            }
        }
    }
}
