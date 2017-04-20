using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Piet
{
    // tutorial: http://homepages.vub.ac.be/~diddesen/piet/index.html

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Interpreter _interpreter;

        public MainWindow()
        {
            InitializeComponent();

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            //string filename = @"C:\Prj\Personal\Esolang\Piet\Programs\hello-medium.png";
            //int codelSize = 4;
            string filename = @"C:\Prj\Personal\Esolang\Piet\Programs\nprime.gif";
            int codelSize = 8;
            //string filename = @"C:\Prj\Personal\Esolang\Piet\Programs\primetest2.png";
            //int codelSize = 1;
            //string filename = @"C:\Prj\Personal\Esolang\Piet\Programs\helloworld-pietbig.gif";
            //int codelSize = 4; // codelSize:4 -> Hellow world!    codelSize:8 -> Piet
            //string filename = @"C:\Prj\Personal\Esolang\Piet\Programs\hw1-11.gif";
            //int codelSize = 11;
            //string filename = @"C:\Prj\Personal\Esolang\Piet\Programs\hw3-5.gif";
            //int codelSize = 5;
            //string filename = @"C:\Prj\Personal\Esolang\Piet\Programs\erat2.png"; // ??? should display prime numbers
            //int codelSize = 1;
            //string filename = @"C:\Prj\Personal\Esolang\Piet\Programs\euclid_clint.png";
            //int codelSize = 1;
            //string filename = @"C:\Prj\Personal\Esolang\Piet\Programs\piet_factorial_big.png"; // bugged
            //int codelSize = 10;
            //string filename = @"C:\Prj\Personal\Esolang\Piet\Programs\dayofweek.png"; // bugged
            //int codelSize = 1;
            //string filename = @"C:\Prj\Personal\Esolang\Piet\Programs\sqr-art-cs20.png"; // from http://homepages.vub.ac.be/~diddesen/piet/index.html
            //string filename = @"C:\Prj\Personal\Esolang\Piet\Programs\sqr-cs20.png";
            //int codelSize = 20;

            // Display original image
            OriginalImage.Source = new BitmapImage(new Uri(filename, UriKind.Absolute));

            // Parse image
            _interpreter = new Interpreter(InputFunc, OutputAction);
            _interpreter.Parse(filename, codelSize);

            // Display parsed image
            try
            {
                ParsedImage.Source = _interpreter.GenerateImageFromCodels();
            }
            catch (Exception ex)
            {
            }
        }

        private void OutputAction(string s)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                OutputTextBlock.Text += s;
            }, DispatcherPriority.Background);
        }

        private string InputFunc()
        {
            InputWindow inputWindow = new InputWindow();
            inputWindow.ShowDialog();
            return inputWindow.InputTextBox.Text;
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            // Execute code
            //Task.Factory.StartNew(() => _interpreter.Execute());
            _interpreter.Execute();
        }
    }
}
