using Sando.UI.View.Navigator;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Interaction logic for RelatedItemsWindow.xaml
    /// </summary>
    public partial class RelatedItemsWindow : Window
    {
        private double posX;
        private double posY;
        public RelatedItemsWindow()
        {
            InitializeComponent();            
        }

        private void CloseWindow()
        {
            var items = this.Content as RelatedItems;
            if (items != null)
                items.Dispose();
            Close();
        }

        private void RelatedItems_KeyDown_1(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                CloseWindow();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {            
            if (posX < 0 || posX > this.Width || posY < 0 || posY > this.Height)
                this.Close();
        }


     
        private void Window_MouseMove(object sender, MouseEventArgs e)
        {            
            Point p = e.GetPosition(this);
            posX = p.X; 
            posY = p.Y; 
        }

        private void Window_Loaded(object sender, EventArgs e)
        {
            System.Windows.Input.Mouse.Capture(this, System.Windows.Input.CaptureMode.SubTree);                        
        }

        private void Window_LostFocus_1(object sender, EventArgs e)
        {
            (this.Content as RelatedItems).NeedsElement = false;            
        }

        private void Window_GotFocus_1(object sender, EventArgs e)
        {
            (this.Content as RelatedItems).NeedsElement = true;            
        }

     

        

     


    }
}
