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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Sando.ExtensionContracts.ProgramElementContracts;
using Sando.ExtensionContracts.ResultsReordererContracts;

namespace LocalSearch.View
{
    /// <summary>
    /// Interaction logic for NavigationBoxes.xaml
    /// </summary>
    public partial class NavigationBoxes : UserControl
    {
        public NavigationBoxes()
        {
            this.DataContext = this;
            FirstProgramElements = new ObservableCollection<CodeSearchResult>();
            SecondProgramElements = new ObservableCollection<CodeSearchResult>();
            InitializeComponent();
        }

        public ObservableCollection<CodeSearchResult> FirstProgramElements
        {
            get
            {
                return (ObservableCollection<CodeSearchResult>)GetValue(FirstProgramElementsProperty);
            }
            set
            {
                SetValue(FirstProgramElementsProperty, value);
            }
        }

        public static readonly DependencyProperty FirstProgramElementsProperty =
                DependencyProperty.Register("FirstProgramElements", typeof(ObservableCollection<CodeSearchResult>), typeof(NavigationBoxes), new UIPropertyMetadata(null));

        public ObservableCollection<CodeSearchResult> SecondProgramElements
        {
            get
            {
                return (ObservableCollection<CodeSearchResult>)GetValue(SecondProgramElementsProperty);
            }
            set
            {
                SetValue(SecondProgramElementsProperty, value);
            }
        }

        public static readonly DependencyProperty SecondProgramElementsProperty =
                DependencyProperty.Register("SecondProgramElements", typeof(ObservableCollection<CodeSearchResult>), typeof(NavigationBoxes), new UIPropertyMetadata(null));


        private void FirstProgramElements_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

            SecondProgramElements.Clear();
            var methods = InformationSource.GetRelatedMethods(FirstProgramElementsList.SelectedItem as CodeSearchResult);
            foreach (var method in methods)
            {
                SecondProgramElements.Add(method);
            }
        }

        private void SecondProgramElements_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }

        public GraphBuilder InformationSource = null;

    }
}
