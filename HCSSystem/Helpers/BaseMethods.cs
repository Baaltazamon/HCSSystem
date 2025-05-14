using System.Windows;

namespace HCSSystem.Helpers
{
    public static class BaseMethods
    {
        public static bool IsTextAllowed(string text)
        {
            return text.All(c => char.IsDigit(c) || c is '.' or ',');
        }

        public static void HandleTextBoxPasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(DataFormats.Text))
            {
                var text = e.DataObject.GetData(DataFormats.Text) as string;
                if (!IsTextAllowed(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }
    }
}
