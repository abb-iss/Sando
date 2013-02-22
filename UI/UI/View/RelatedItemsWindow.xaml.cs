using Sando.UI.View.Navigator;
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
    /// Interaction logic for RelatedItemsWindow.xaml
    /// </summary>
    public partial class RelatedItemsWindow : Window
    {
        public RelatedItemsWindow()
        {
            InitializeComponent();
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var items = this.Content as RelatedItems;
            if (items != null)
                items.Dispose();
            Close();
        }

        private void RelatedItems_KeyDown_1(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                CommandBinding_Executed(null, null);
        }
    }
}
