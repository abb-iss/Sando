using System;
using System.Collections.Generic;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Sando.ExtensionContracts.ProgramElementContracts;

namespace Sando.UI.View.Search.Converters
{
    [ValueConversion(typeof(string), typeof(BitmapImage))] 
    public class ElementToIcon : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return null;

            var element = value as ProgramElement;
            if (element == null)
                return GetBitmapImage("../Resources/VS2010Icons/generic.png");

            if(element as CommentElement != null)
                return GetBitmapImage("../Resources/VS2010Icons/comment.png");

            if (element as XmlXElement != null)
                return GetBitmapImage("../Resources/VS2010Icons/VSObject_XElement.png");


            string accessLevel;
            var info = element.GetType().GetProperty("AccessLevel");
            if (info != null)
                accessLevel = "_" + info.GetValue(element, null);
            else
                accessLevel = string.Empty;
            
            if (accessLevel.ToLower() == "_public")
                accessLevel = "";
            if (accessLevel.ToLower() == "_internal")
                accessLevel = "_Private";

            var programElementType = element.ProgramElementType;
            if(programElementType == ProgramElementType.MethodPrototype)
                programElementType = ProgramElementType.Method;

            if (programElementType == ProgramElementType.Field)
                programElementType = ProgramElementType.Property;

            string resourceName;
            if (programElementType == ProgramElementType.Struct)
                resourceName = String.Format("../Resources/VS2010Icons/VSObject_{0}{1}.png", "Structure", accessLevel);
            else if(programElementType == ProgramElementType.TextLine)
                resourceName = "../Resources/VS2010Icons/xmlIcon.png";
            else
                resourceName = string.Format("../Resources/VS2010Icons/VSObject_{0}{1}.png", programElementType, accessLevel);

            return GetBitmapImage(resourceName);
        }

        static readonly Dictionary<string,BitmapImage> Images = new Dictionary<string, BitmapImage>();

        private static BitmapImage GetBitmapImage(string resource)
        {
            BitmapImage image;
            if (Images.TryGetValue(resource, out image))
                return image;
            
            image = new BitmapImage(new Uri(resource, UriKind.Relative));
            Images[resource] = image;
            return image;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return new NotImplementedException();
        }
    }
}