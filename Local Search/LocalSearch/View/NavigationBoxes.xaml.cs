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
            ThirdProgramElements = new ObservableCollection<CodeSearchResult>();
            FourthProgramElements = new ObservableCollection<CodeSearchResult>();
            FifthProgramElements = new ObservableCollection<CodeSearchResult>();
            SixthProgramElements = new ObservableCollection<CodeSearchResult>();
            SeventhProgramElements = new ObservableCollection<CodeSearchResult>();
            InitializeComponent();
        }

        public static readonly DependencyProperty FirstProgramElementsProperty =
                DependencyProperty.Register("FirstProgramElements", typeof(ObservableCollection<CodeSearchResult>), typeof(NavigationBoxes), new UIPropertyMetadata(null));

        public static readonly DependencyProperty SecondProgramElementsProperty =
                DependencyProperty.Register("SecondProgramElements", typeof(ObservableCollection<CodeSearchResult>), typeof(NavigationBoxes), new UIPropertyMetadata(null));

        public static readonly DependencyProperty ThirdProgramElementsProperty =
                DependencyProperty.Register("ThirdProgramElements", typeof(ObservableCollection<CodeSearchResult>), typeof(NavigationBoxes), new UIPropertyMetadata(null));

        public static readonly DependencyProperty FourthProgramElementsProperty =
                DependencyProperty.Register("FourthProgramElements", typeof(ObservableCollection<CodeSearchResult>), typeof(NavigationBoxes), new UIPropertyMetadata(null));

        public static readonly DependencyProperty FifthProgramElementsProperty =
                DependencyProperty.Register("FifthProgramElements", typeof(ObservableCollection<CodeSearchResult>), typeof(NavigationBoxes), new UIPropertyMetadata(null));

        public static readonly DependencyProperty SixthProgramElementsProperty =
                DependencyProperty.Register("SixthProgramElements", typeof(ObservableCollection<CodeSearchResult>), typeof(NavigationBoxes), new UIPropertyMetadata(null));

        public static readonly DependencyProperty SeventhProgramElementsProperty =
                DependencyProperty.Register("SeventhProgramElements", typeof(ObservableCollection<CodeSearchResult>), typeof(NavigationBoxes), new UIPropertyMetadata(null));


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

        public ObservableCollection<CodeSearchResult> ThirdProgramElements
        {
            get
            {
                return (ObservableCollection<CodeSearchResult>)GetValue(ThirdProgramElementsProperty);
            }
            set
            {
                SetValue(ThirdProgramElementsProperty, value);
            }
        }

        public ObservableCollection<CodeSearchResult> FourthProgramElements
        {
            get
            {
                return (ObservableCollection<CodeSearchResult>)GetValue(FourthProgramElementsProperty);
            }
            set
            {
                SetValue(FourthProgramElementsProperty, value);
            }
        }

        public ObservableCollection<CodeSearchResult> FifthProgramElements
        {
            get
            {
                return (ObservableCollection<CodeSearchResult>)GetValue(FifthProgramElementsProperty);
            }
            set
            {
                SetValue(FifthProgramElementsProperty, value);
            }
        }

        public ObservableCollection<CodeSearchResult> SixthProgramElements
        {
            get
            {
                return (ObservableCollection<CodeSearchResult>)GetValue(SixthProgramElementsProperty);
            }
            set
            {
                SetValue(SixthProgramElementsProperty, value);
            }
        }

        public ObservableCollection<CodeSearchResult> SeventhProgramElements
        {
            get
            {
                return (ObservableCollection<CodeSearchResult>)GetValue(SeventhProgramElementsProperty);
            }
            set
            {
                SetValue(SeventhProgramElementsProperty, value);
            }
        }   

        private void FirstProgramElements_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

            SecondProgramElements.Clear();
            //var methods = InformationSource.GetRelatedMethods(FirstProgramElementsList.SelectedItem as CodeSearchResult);
            var relatedmembers = InformationSource.GetRelatedInfo(FirstProgramElementsList.SelectedItem as CodeSearchResult);
            foreach (var member in relatedmembers)
            {
                CodeSearchResult memberAsSearchResult = new CodeSearchResult(member.Element, 1.0);
                SecondProgramElements.Add(memberAsSearchResult);
            }
        }

        private void SecondProgramElements_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ThirdProgramElements.Clear();
            var relatedmembers = InformationSource.GetRelatedInfo(SecondProgramElementsList.SelectedItem as CodeSearchResult);
            foreach (var member in relatedmembers)
            {
                CodeSearchResult memberAsSearchResult = new CodeSearchResult(member.Element, 1.0);
                ThirdProgramElements.Add(memberAsSearchResult);
            }
        }

        private void ThirdProgramElements_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            FourthProgramElements.Clear();
            var relatedmembers = InformationSource.GetRelatedInfo(ThirdProgramElementsList.SelectedItem as CodeSearchResult);
            foreach (var member in relatedmembers)
            {
                CodeSearchResult memberAsSearchResult = new CodeSearchResult(member.Element, 1.0);
                FourthProgramElements.Add(memberAsSearchResult);
            }
        }

        private void FourthProgramElements_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            FifthProgramElements.Clear();
            var relatedmembers = InformationSource.GetRelatedInfo(FourthProgramElementsList.SelectedItem as CodeSearchResult);
            foreach (var member in relatedmembers)
            {
                CodeSearchResult memberAsSearchResult = new CodeSearchResult(member.Element, 1.0);
                FifthProgramElements.Add(memberAsSearchResult);
            }
        }

        private void FifthProgramElements_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            SixthProgramElements.Clear();
            var relatedmembers = InformationSource.GetRelatedInfo(FifthProgramElementsList.SelectedItem as CodeSearchResult);
            foreach (var member in relatedmembers)
            {
                CodeSearchResult memberAsSearchResult = new CodeSearchResult(member.Element, 1.0);
                SixthProgramElements.Add(memberAsSearchResult);
            }
        }

        private void SixthProgramElements_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            SeventhProgramElements.Clear();
            var relatedmembers = InformationSource.GetRelatedInfo(SixthProgramElementsList.SelectedItem as CodeSearchResult);
            foreach (var member in relatedmembers)
            {
                CodeSearchResult memberAsSearchResult = new CodeSearchResult(member.Element, 1.0);
                SeventhProgramElements.Add(memberAsSearchResult);
            }
        }

        private void SeventhProgramElements_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            //todo
        }

        public GraphBuilder InformationSource = null;

    }
}
