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

        public MessageControl()
        {
            InitializeComponent();
        }

        public MessageControl(string text) : this()
        {
            Text = text;
            Task.Run(() => PlayAnimation(text));
        }

        public MessageControl(string text, bool animation) : this()
        {
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

        public void AddString(string text)
        {
            animationTextBox.AddString(text);
        }
    }
}
