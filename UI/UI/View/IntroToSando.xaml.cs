using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Sando.UI.View
{
    /// <summary>
    /// Interaction logic for IntroToSando.xaml
    /// </summary>
    public partial class IntroToSando : Window
    {
        public IntroToSando()
        {
            UploadAllowed = true;
            InitializeComponent();
            this.Icon = new BitmapImage(new Uri("sandosearch.bmp", UriKind.Relative));
        }

        public bool UploadAllowed { get; set; }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            UploadAllowed = !UploadAllowed;
        }
    }
}
