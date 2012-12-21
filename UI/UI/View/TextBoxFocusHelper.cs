using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace FocusTestVC
{
    /// <summary>
    /// Implements a feature to select all text within a TextBox when focus is set.
    /// </summary>
    public static class TextBoxFocusHelper
    {

        #region " Declarations "
        /// <summary>
        /// Whether or not a textbox that is unloaded will automatically be unregistered from this helper.
        /// </summary>
        public static bool UnregisterOnUnload { get; set; }

        /// <summary>
        /// Contains a list of all textbox controls that this helper has attached to.
        /// The Boolean value is used in event handling to determine if mouse capture
        /// should be ignored.
        /// </summary>

        private static Dictionary<TextBox, bool> FocusTextBoxes = new Dictionary<TextBox, bool>();
        /// <summary>
        /// Delegate used to invoke the OnFocusComplete method.
        /// </summary>
        /// <param name="txt">The textbox that the OnFocusComplete method is being invoked for.</param>
        private delegate void OnFocusCompleteDelegate(TextBox txt);

        /// <summary>
        /// Used to invoke the OnFocusCompleteDelegate delegate.
        /// </summary>
        private static OnFocusCompleteDelegate OnFocusCompleteInvoker = new OnFocusCompleteDelegate(OnFocusComplete);

        #endregion

        #region " Public Methods "
        /// <summary>
        /// Attaches a TextBox control to the helper.  While a textbox is attached (registered), it will
        /// automatically select all text when focus is set.
        /// </summary>
        /// <param name="txt">The TextBox control to register.</param>
        public static void RegisterFocus(TextBox txt)
        {
            if (txt == null)
                throw new ArgumentNullException("txt");
            if (FocusTextBoxes.ContainsKey(txt))
                return;

            FocusTextBoxes.Add(txt, false);

            txt.GotKeyboardFocus += TextBox_GotKeyboardFocus;
            txt.GotMouseCapture += TextBox_GotMouseCapture;
            txt.Unloaded += TextBox_Unloaded;
        }

        /// <summary>
        /// Removes a TextBox control from the helper.  The textbox will no longer
        /// automatically select all text when focus is set.
        /// </summary>
        /// <param name="txt">The TextBox control to unregister.</param>
        public static void UnregisterFocus(TextBox txt)
        {
            if (txt == null)
                throw new ArgumentNullException("txt");

            try
            {
                txt.GotKeyboardFocus -= TextBox_GotKeyboardFocus;
            }
            catch { }

            try
            {
                txt.GotMouseCapture -= TextBox_GotMouseCapture;
            }
            catch { }

            try
            {
                txt.Unloaded -= TextBox_Unloaded;
            }
            catch { }

            if (FocusTextBoxes.ContainsKey(txt))
                FocusTextBoxes.Remove(txt);
        }
        #endregion

        #region " TextBox Event Handlers "
        /// <summary>
        /// Handles TextBox.GotKeyboardFocus event.
        /// </summary>
        private static void TextBox_GotKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            TextBox txt = sender as TextBox;
            if (txt == null)
                return;
            if (!FocusTextBoxes.ContainsKey(txt))
                return;

            // Select all of the text in the control.
            txt.SelectAll();

            // Allow the TextBox.GotMouseCapture custom code to execute.
            FocusTextBoxes[txt] = true;

            // Invoke the OnFocusComplete method.  The method should run as the last step
            // of the textbox focusing logic.
            txt.Dispatcher.BeginInvoke(OnFocusCompleteInvoker, System.Windows.Threading.DispatcherPriority.Input, txt);
        }

        /// <summary>
        /// Handles TextBox.GotMouseCapture event.
        /// </summary>
        private static void TextBox_GotMouseCapture(object sender, System.Windows.Input.MouseEventArgs e)
        {
            TextBox txt = sender as TextBox;
            if (txt == null)
                return;
            if (!FocusTextBoxes.ContainsKey(txt))
                return;

            // Check that this code should execute.

            if (FocusTextBoxes[txt])
            {
                // Select all of the text in the control.
                // It would seem this call is redundant however it is necessary to handle the case where the
                // user has clicked the mouse outside of the text region and within the textbox (in other words,
                // the user clicked within the whitespace).
                txt.SelectAll();

                // Release mouse capture will prevent the textbox from unselecting the text during the TextBox.MouseUp event.
                txt.ReleaseMouseCapture();

                // Prevent this function from executing again (until the next focus event).
                // If this code were allowed to continuously execute, the user would not be able to change the selection.
                FocusTextBoxes[txt] = false;
            }
        }

        /// <summary>
        /// Prevent the TextBox.GotMouseCapture custom code from firing until the next focus event.
        /// This method should be invoked after the TextBox has finalized its focus logic.
        /// </summary>
        /// <param name="helper">The TextBoxFocusHelper that invoked this method.</param>
        /// <param name="txt">The TextBox that completed the focus logic.</param>
        private static void OnFocusComplete(TextBox txt)
        {
            if (!FocusTextBoxes.ContainsKey(txt))
                return;

            FocusTextBoxes[txt] = false;
        }

        /// <summary>
        /// Handles TextBox.Unloaded event.
        /// Unregisters the textbox from this helper if UnregisterOnUnload is enabled,
        /// otherwise does nothing.
        /// </summary>
        private static void TextBox_Unloaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!UnregisterOnUnload)
                return;

            TextBox txt = sender as TextBox;
            if (txt == null)
                return;

            UnregisterFocus(txt);
        }

        #endregion

    }
}