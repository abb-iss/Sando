using System;
using System.IO;
using System.Windows.Forms;
using Fusion8.Cropper.Extensibility;



namespace Fusion8.Cropper.Core
{
    public class FileNameTemplate
    {
		private string fileName;
        private int lastIncrement = 1;

        public ImagePairNames Parse(string extension)
        {
            fileExtension = extension;
            string fullImageTemplate = GetFullImageTemplate();
            string thumbImageTemplate = GetThumbImageTemplate();

            ImagePairNames names = new
                ImagePairNames(fullImageTemplate, thumbImageTemplate);

            names = TryAddTemplateDateOrTime(names);
            names = TryAddTemplateExtension(names);
            names = TryAddTemplateUser(names);
            names = TryAddTemplateDomain(names);
            names = TryAddTemplateMachine(names);
            names = SetFileExtension(names);
            names = SetFullPath(names);
            names = TryAddTemplatePrompt(names);
            names = GetNextImagePairNames(names);
            return names;
        }

        public void ResetIncrement()
        {
            this.lastIncrement = 1;
        }

        private static string GetFullImageTemplate()
        {
            string fullImageTemplate;
            if (Configuration.Current.FullImageTemplate != null && Configuration.Current.FullImageTemplate.Length > 0)
                fullImageTemplate = Configuration.Current.FullImageTemplate;
            else
                fullImageTemplate = DefaultFullImageTemplate;
            return fullImageTemplate;
        }

        private static ImagePairNames TryAddTemplatePrompt(ImagePairNames startNames)
        {
            if (startNames.FullSize.IndexOf(Templates.Prompt) >= 0 || startNames.Thumbnail.IndexOf(Templates.Prompt) >= 0)
            {
                Prompt prompt = new Prompt();
                prompt.TopMost = true;
                prompt.StartPosition = FormStartPosition.CenterParent;
                if (prompt.ShowDialog(Configuration.Current.ActiveCropWindow) == DialogResult.OK)
                {
                    startNames.FullSize = startNames.FullSize.Replace(Templates.Prompt, prompt.Value);
                    startNames.Thumbnail = startNames.Thumbnail.Replace(Templates.Prompt, prompt.Value);
                }
                else
                {
                    startNames.FullSize = startNames.FullSize.Replace(Templates.Prompt, string.Empty);
                    startNames.Thumbnail = startNames.Thumbnail.Replace(Templates.Prompt, String.Empty);
                }
            }
            return startNames;
        }
    }
}
