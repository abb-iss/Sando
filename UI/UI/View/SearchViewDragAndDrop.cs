using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Sando.ExtensionContracts.DataFormats;
using Sando.ExtensionContracts.ResultsReordererContracts;

namespace Sando.UI.View
{
    public partial class SearchViewControl 
    {
        private void listView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var listBoxItem = sender as ListBoxItem;
            if (listBoxItem == null) return;

            var codeSearchResult = listBoxItem.Content as CodeSearchResult;
            if (codeSearchResult == null) return;

            var dragData = new DataObject(SandoDataFormats.SandoSearchResult, codeSearchResult);
            DragDrop.DoDragDrop((DependencyObject)sender, dragData, DragDropEffects.All);
        }
    }
}
