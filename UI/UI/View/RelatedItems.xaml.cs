﻿using EnvDTE80;
using LocalSearch;
using Sando.DependencyInjection;
using Sando.ExtensionContracts.ProgramElementContracts;
using Sando.ExtensionContracts.ResultsReordererContracts;
using Sando.UI.Actions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;


namespace Sando.UI.View.Navigator
{
    /// <summary>
    /// Interaction logic for RelatedItems.xaml
    /// </summary>
    public partial class RelatedItems : UserControl
    {
        public RelatedItems()
        {
            DataContext = this; //so we can show results
            relatedItems = new ObservableCollection<CodeSearchResult>();
            InitializeComponent();
            NeedsElement = true;
        }

        public ObservableCollection<CodeSearchResult> relatedItems
        {
            get { return (ObservableCollection<CodeSearchResult>)GetValue(relatedItemsProperty); }
            set { SetValue(relatedItemsProperty, value); }
        }

        public static readonly DependencyProperty relatedItemsProperty =
                DependencyProperty.Register("relatedItems", typeof(ObservableCollection<CodeSearchResult>), typeof(RelatedItems), new UIPropertyMetadata(null));
        

        public LocalSearch.Context Context { get; set; }

        private void relatedItemsListbox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if (lastOne != null)
            {
                lastOne.Dispose();
            }
            var listBox = relatedItemsListbox;
            var active = ServiceLocator.Resolve<DTE2>().ActiveDocument;
            var fileName = "";
            if (active != null)
            {
                fileName = Path.Combine(active.Path, active.Name);
            }
            Window parentWindow = Window.GetWindow(this);
            lastOne = RecommendationShower.Create(listBox, fileName, Dispatcher).SetContext(Context).SetY(parentWindow.Top);
            lastOne.Show();
            SetCurrent(listBox.SelectedItem as CodeSearchResult);
            ShowItem();            
        }

        private void ShowItem()
        {            
            var navigationResult = relatedItemsListbox.SelectedItem as ProgramElementWithRelation;
            if (navigationResult != null)
            {
                FileOpener.OpenFile(navigationResult.ProgramElement.FullFilePath, navigationResult.RelationLineNumber[0], "", true);                
            }
            else
            {
                var searchResult = relatedItemsListbox.SelectedItem as CodeSearchResult;
                FileOpener.OpenItem(searchResult, "", true);
            }
        }

        public string CurrentName
        {
            get { return (string)GetValue(CurrentNameProperty); }
            private set { SetValue(CurrentNameProperty, value); }
        }

        public static readonly DependencyProperty CurrentNameProperty =
            DependencyProperty.Register("CurrentName", typeof(string), typeof(RelatedItems), new UIPropertyMetadata(null));


        public ProgramElement CurrentElement
        {
            get { return (ProgramElement)GetValue(CurrentElementProperty); }
            private set { SetValue(CurrentElementProperty, value); }
        }

        public static readonly DependencyProperty CurrentElementProperty =
            DependencyProperty.Register("CurrentElement", typeof(ProgramElement), typeof(RelatedItems), new UIPropertyMetadata(null));


        public bool NeedsElement
        {
            get { return (bool)GetValue(NeedsElementProperty); }
            private set { SetValue(NeedsElementProperty, value); }
        }

        public static readonly DependencyProperty NeedsElementProperty =
            DependencyProperty.Register("NeedsElement", typeof(bool), typeof(RelatedItems), new UIPropertyMetadata(null));


        public string CurrentRelation
        {
            get { return (string)GetValue(CurrentRelationProperty); }
            private set { SetValue(CurrentRelationProperty, value); }
        }

        public static readonly DependencyProperty CurrentRelationProperty =
            DependencyProperty.Register("CurrentRelation", typeof(string), typeof(RelatedItems), new UIPropertyMetadata(null));
        private RecommendationShower lastOne;



        public void SetCurrent(CodeSearchResult cs1)
        {
            CurrentElement = cs1.ProgramElement;
            CurrentName = cs1.Name;
            if (cs1 as ProgramElementWithRelation != null)
                CurrentRelation = (cs1 as ProgramElementWithRelation).ProgramElementRelationString;
            NeedsElement = false;
        }

        internal void Dispose()
        {
            if (lastOne != null)
                lastOne.Dispose();
        }


    }
}
