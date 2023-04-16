using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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

        public MessageControl(string text, bool animation = false, Brush? foreground = default) : this()
        {
            Text = text;
            if (foreground != null)
            {
                Foreground = foreground;
            }

            if (animation)
            {
                Task.Run(() => PlayAnimation(text));
            }
            else
            {
                animationTextBox.Text = text;
            }
        }

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set
            {
                SetValue(TextProperty, value);
                animationTextBox.Text = value;
            }
        }

        public async Task PlayAnimation(string text)
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
