using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace HybridAI.Control.Chat
{
    /// <summary>
    /// Message.xaml 的交互逻辑
    /// </summary>
    public partial class MessageControl : UserControl
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(MessageControl));

        public MessageControl(string text)
        {
            InitializeComponent();
            Text = text;
            Task.Run(() => PlayAnimation(text));
        }

        public MessageControl(string text, bool animation)
        {
            InitializeComponent();
            Text = text;

            if (animation)
            {
                Task.Run(() => PlayAnimation(text));
            }
            else
            {
                animationTextBox.AddString(text);
            }
        }

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public async void PlayAnimation(string text)
        {
            foreach (char character in text)
            {
                await Dispatcher.BeginInvoke(() => animationTextBox.AddString(character.ToString()));
                await Task.Delay(10);
            }
        }
    }
}
