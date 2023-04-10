using System.Windows;
using System.Windows.Controls;

namespace HybridAI.Control.Chat
{
    /// <summary>
    /// ResponseControl.xaml 的交互逻辑
    /// </summary>
    public partial class ResponseControl : UserControl
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), ownerType: typeof(ResponseControl));

        public ResponseControl(string text)
        {
            InitializeComponent();
            Text = text;
        }

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
    }
}
