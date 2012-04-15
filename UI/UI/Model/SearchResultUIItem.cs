using System;
using System.Reflection;
using System.Windows;
using System.Windows.Media.Imaging;
using Sando.ExtensionContracts.ResultsReordererContracts;

namespace Sando.UI.Model
{
	public class SearchResultUIItem
	{
		#region Properties Definitions

		public string Name
		{
			get
			{
				return CodeSearchResult.Name;
			}
		}

		public string FileName
		{
			get
			{
				return CodeSearchResult.FileName;
			}
		}

		public string Parent
		{
			get
			{
				return CodeSearchResult.Parent;
			}
		}

		public string Snippet
		{
			get
			{
				return CodeSearchResult.Snippet;
			}
		}

		public string ElementType
		{
			get
			{
				return CodeSearchResult.Type;
			}
		}

		public int DefinitionLineNumber
		{
			get
			{
				return CodeSearchResult.Element.DefinitionLineNumber;
			}
		}

		public string AccessLevel
		{
			get
			{
				string accessLevel;
				PropertyInfo info = CodeSearchResult.Element.GetType().GetProperty("AccessLevel");
				if (info != null)
				{
					accessLevel = info.GetValue(CodeSearchResult.Element, null).ToString();
				}
				else
				{
					accessLevel = string.Empty;
				}
				return accessLevel;
			}
		}

		private string _AccessLevel
		{
			get { return (AccessLevel == string.Empty ? "" : "_") + AccessLevel; }
		}

		public BitmapImage IconSelf
		{
			get
			{
				ResourceDictionary res = new ResourceDictionary();
				
				string iconPath;
				
				iconPath = string.Format("VSObject_{0}{1}.png", ElementType, _AccessLevel);
				BitmapImage icon = new BitmapImage(new Uri(iconPath, UriKind.Relative));
				//Application.Current.TryFindResource(iconPath) as BitmapImage;
				
				try
				{
					if (icon.StreamSource.Length==0);
					if (icon.Height == 0)
						icon = null;
				}
				catch
				{
					iconPath = string.Format("{0}VSObject_{1}.png", Constant.IconPath, ElementType);
					icon = new BitmapImage(new Uri(iconPath, UriKind.Relative));
					try
					{
						if (icon.Height == 0)
							icon = null;
					}
					catch
					{
						iconPath = Constant.DefaultIcon;
						icon = new BitmapImage(new Uri(iconPath, UriKind.Relative));
					}
				}
				return icon;
			}
		}

		public BitmapImage IconFile
		{
			get
			{
				return new BitmapImage(new Uri(Constant.IconFile, UriKind.Relative));
			}
		}

		#endregion

		#region Constructor

		public SearchResultUIItem(CodeSearchResult codeSearchResult)
		{
			this.CodeSearchResult = codeSearchResult;
		}

		#endregion

		#region members

		public CodeSearchResult CodeSearchResult
		{
			get;
			private set;
		}

		#endregion
	}

	class Constant
	{
		public static string ResourcePath
		{
			get { return "../Resources/"; }
		}

		public static string IconPath
		{
			get { return ResourcePath + "VS2010Icons/"; }
		}

		public static string DefaultIcon
		{
			get { return IconPath + "VSObject_Object.png"; }
		}

		public static string IconFile
		{
			get { return ResourcePath + "IconFile.png"; }
		}
	}
}
