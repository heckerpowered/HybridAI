using System.Windows;
using System.Windows.Controls;

namespace HybridAI.Control.Chat
{
    /// <summary>
    /// ErrorControl.xaml 的交互逻辑
    /// </summary>
    public partial class ErrorControl : UserControl
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(ErrorControl));
        public ErrorControl(string text)
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
