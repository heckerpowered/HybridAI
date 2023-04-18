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
        private readonly MessageKind messageKind;
        private readonly Task? animationPerformance;
        public MessageControl()
        {
            InitializeComponent();
        }

        public MessageControl(MessageBuilder builder) : this()
        {
            var text = builder.text;
            var performAnimation = builder.performAnimation;
            var foreground = builder.foreground;
            var messageKind = builder.kind;

            Text = text;
            this.messageKind = messageKind;
            if (foreground != null)
            {
                Foreground = foreground;
                animatedTextBlock.Foreground = foreground;
            }

            if (performAnimation)
            {
                animationPerformance = Task.Run(() => PerformAnimation(text));
            }
            else
            {
                animatedTextBlock.Text = text;
            }
        }

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set
            {
                SetValue(TextProperty, value);
                animatedTextBlock.Text = value;
            }
        }

        public async Task PerformAnimation(string text)
        {
            foreach (char character in text)
            {
                await Dispatcher.BeginInvoke(() => animatedTextBlock.AddString(character.ToString()));
                await Task.Delay(10);
            }
        }

        public Task? GetAnimationPerformance()
        {
            return animationPerformance;
        }

        public void AddString(string text)
        {
            animatedTextBlock.AddString(text);
        }

        private void Retry(object sender, RoutedEventArgs e)
        {

        }

        private void MenuItemLoaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
