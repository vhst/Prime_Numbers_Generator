using System;
using System.Globalization;
using System.Windows;
using System.Windows.Threading;
using W3b.Sine;

namespace Prime_Numbers_Generator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ShowComputing()
        {
            this.IsEnabled = false;
            computingWindow.Visibility = Visibility.Visible;
            Dispatcher.Invoke(DispatcherPriority.Render, (Action)(() => { UpdateLayout(); }));
        }

        private void CloseComputing()
        {
            this.IsEnabled = true;
            computingWindow.Visibility = Visibility.Hidden;
            Dispatcher.Invoke(DispatcherPriority.Render, (Action)(() => { UpdateLayout(); }));
        }

        private void GenerateButton_Click_1(object sender, RoutedEventArgs e)
        {
            ShowComputing();
            BigNumDec prob;

            try
            {
                prob = new BigNumDec(probabilityTextBox.Text.Replace(',', '.'));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Probability is entered incorrectly", Title, MessageBoxButton.OK, MessageBoxImage.Error);
                CloseComputing();
                return;
            }

            int n;
            int b;
            try
            {
                n = Int32.Parse(nTextBox.Text, new CultureInfo("ru-RU"));
                b = Int32.Parse(bTextBox.Text, new CultureInfo("ru-RU"));
            }
            catch (Exception ex)
            {
                MessageBox.Show("N or B is entered incorrectly", Title, MessageBoxButton.OK, MessageBoxImage.Error);
                CloseComputing();
                return;
            }

            Generator gen = new Generator();
            try
            {
                var primeNumbers = gen.Next(n, b, prob);
                pTextBox.Text = primeNumbers.Item1.ToString();
                qTextBox.Text = primeNumbers.Item2.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error occurred: " + ex.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            CloseComputing();
        }
    }
}
