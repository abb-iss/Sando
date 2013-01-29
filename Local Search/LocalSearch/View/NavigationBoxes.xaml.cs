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
            FirstProgramElements = new ObservableCollection<ProgramElementWithRelation>();
            SecondProgramElements = new ObservableCollection<ProgramElementWithRelation>();
            ThirdProgramElements = new ObservableCollection<ProgramElementWithRelation>();
            FourthProgramElements = new ObservableCollection<ProgramElementWithRelation>();
            FifthProgramElements = new ObservableCollection<ProgramElementWithRelation>();
            SixthProgramElements = new ObservableCollection<ProgramElementWithRelation>();
            SeventhProgramElements = new ObservableCollection<ProgramElementWithRelation>();
            SelectedElements = new List<ProgramElementWithRelation>();
            InitializeComponent();
        }

        public static readonly DependencyProperty FirstProgramElementsProperty =
                DependencyProperty.Register("FirstProgramElements", typeof(ObservableCollection<ProgramElementWithRelation>), typeof(NavigationBoxes), new UIPropertyMetadata(null));

        public static readonly DependencyProperty SecondProgramElementsProperty =
                DependencyProperty.Register("SecondProgramElements", typeof(ObservableCollection<ProgramElementWithRelation>), typeof(NavigationBoxes), new UIPropertyMetadata(null));

        public static readonly DependencyProperty ThirdProgramElementsProperty =
                DependencyProperty.Register("ThirdProgramElements", typeof(ObservableCollection<ProgramElementWithRelation>), typeof(NavigationBoxes), new UIPropertyMetadata(null));

        public static readonly DependencyProperty FourthProgramElementsProperty =
                DependencyProperty.Register("FourthProgramElements", typeof(ObservableCollection<ProgramElementWithRelation>), typeof(NavigationBoxes), new UIPropertyMetadata(null));

        public static readonly DependencyProperty FifthProgramElementsProperty =
                DependencyProperty.Register("FifthProgramElements", typeof(ObservableCollection<ProgramElementWithRelation>), typeof(NavigationBoxes), new UIPropertyMetadata(null));

        public static readonly DependencyProperty SixthProgramElementsProperty =
                DependencyProperty.Register("SixthProgramElements", typeof(ObservableCollection<ProgramElementWithRelation>), typeof(NavigationBoxes), new UIPropertyMetadata(null));

        public static readonly DependencyProperty SeventhProgramElementsProperty =
                DependencyProperty.Register("SeventhProgramElements", typeof(ObservableCollection<ProgramElementWithRelation>), typeof(NavigationBoxes), new UIPropertyMetadata(null));

        public static readonly DependencyProperty RelationSequenceProperty =
             DependencyProperty.Register("RelationSequence", typeof(string), typeof(NavigationBoxes), new UIPropertyMetadata(null));


        public ObservableCollection<ProgramElementWithRelation> FirstProgramElements
        {
            get
            {
                return (ObservableCollection<ProgramElementWithRelation>)GetValue(FirstProgramElementsProperty);
            }
            set
            {
                SetValue(FirstProgramElementsProperty, value);
            }
        }

        public ObservableCollection<ProgramElementWithRelation> SecondProgramElements
        {
            get
            {
                return (ObservableCollection<ProgramElementWithRelation>)GetValue(SecondProgramElementsProperty);
            }
            set
            {
                SetValue(SecondProgramElementsProperty, value);
            }
        }

        public ObservableCollection<ProgramElementWithRelation> ThirdProgramElements
        {
            get
            {
                return (ObservableCollection<ProgramElementWithRelation>)GetValue(ThirdProgramElementsProperty);
            }
            set
            {
                SetValue(ThirdProgramElementsProperty, value);
            }
        }

        public ObservableCollection<ProgramElementWithRelation> FourthProgramElements
        {
            get
            {
                return (ObservableCollection<ProgramElementWithRelation>)GetValue(FourthProgramElementsProperty);
            }
            set
            {
                SetValue(FourthProgramElementsProperty, value);
            }
        }

        public ObservableCollection<ProgramElementWithRelation> FifthProgramElements
        {
            get
            {
                return (ObservableCollection<ProgramElementWithRelation>)GetValue(FifthProgramElementsProperty);
            }
            set
            {
                SetValue(FifthProgramElementsProperty, value);
            }
        }

        public ObservableCollection<ProgramElementWithRelation> SixthProgramElements
        {
            get
            {
                return (ObservableCollection<ProgramElementWithRelation>)GetValue(SixthProgramElementsProperty);
            }
            set
            {
                SetValue(SixthProgramElementsProperty, value);
            }
        }

        public ObservableCollection<ProgramElementWithRelation> SeventhProgramElements
        {
            get
            {
                return (ObservableCollection<ProgramElementWithRelation>)GetValue(SeventhProgramElementsProperty);
            }
            set
            {
                SetValue(SeventhProgramElementsProperty, value);
            }
        }

        public List<ProgramElementWithRelation> SelectedElements { get; set; }

        public string RelationSequence
        {
            get
            {
                return (string)GetValue(RelationSequenceProperty);
            }
            private set
            {
                SetValue(RelationSequenceProperty, value);
            }
        }

        private void ClearGetAndShow(System.Windows.Controls.ListView currentNavigationBox, ObservableCollection<ProgramElementWithRelation> relatedInfo, int currentPos)
        {
            ClearSelectedElements(currentPos);

            relatedInfo.Clear(); //may triger relatedInfo NavigationBox selection change

            if (currentNavigationBox.SelectedItem != null)
            {
                ProgramElementWithRelation selected = currentNavigationBox.SelectedItem as ProgramElementWithRelation;
                SelectedElements.Add(selected);
                var relatedmembers = InformationSource.GetRelatedInfo(selected);
                foreach (var member in relatedmembers)
                {
                    relatedInfo.Add(member);
                }

                //MessageBox.Show(ShowSequenceOfSelects());
                RelationSequence = ShowSequenceOfSelects();
            }
        }

        private void ClearSelectedElements(int currentPos)
        {
            int SelectedNum = SelectedElements.Count;
            if (SelectedNum > currentPos)
                SelectedElements.RemoveRange(currentPos, SelectedNum-currentPos);
        }

        private String ShowSequenceOfSelects()
        {
            String strbuilder = "";
            int count = SelectedElements.Count;

            if (count == 0)
                return strbuilder;
            
            ProgramElementWithRelation firstElement = SelectedElements[0];
            String NameOfElement = firstElement.Element.Name;
            String TypeOfElement = firstElement.ProgramElementType.ToString();
            strbuilder += TypeOfElement + " \"" + NameOfElement + "\" ";

            int i = 1;
            while (i < count)
            {
                ProgramElementWithRelation Element = SelectedElements[i];
                String Name = Element.Name;
                String Type = Element.ProgramElementType.ToString();

                var type = typeof(ProgramElementRelation);
                var memInfo = type.GetMember(Element.ProgramElementRelation.ToString());
                var attributes = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute),
                    false);
                var description = ((DescriptionAttribute)attributes[0]).Description;

                strbuilder += description + " " + Type + " \"" + Name + "\" ";

                i++;
            }

            return strbuilder.ToString();
        }

        private void FirstProgramElements_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {            
            ClearGetAndShow(FirstProgramElementsList, SecondProgramElements,0);
        }

        private void SecondProgramElements_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ClearGetAndShow(SecondProgramElementsList, ThirdProgramElements,1);
        }

        private void ThirdProgramElements_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ClearGetAndShow(ThirdProgramElementsList, FourthProgramElements,2);
        }

        private void FourthProgramElements_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ClearGetAndShow(FourthProgramElementsList, FifthProgramElements,3);
        }

        private void FifthProgramElements_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ClearGetAndShow(FifthProgramElementsList, SixthProgramElements,4);
        }

        private void SixthProgramElements_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ClearGetAndShow(SixthProgramElementsList, SeventhProgramElements,5);
        }

        private void SeventhProgramElements_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            //MessageBox.Show(ShowSequenceOfSelects());
            RelationSequence = ShowSequenceOfSelects();
        }

        public GraphBuilder InformationSource = null;

    }
}
