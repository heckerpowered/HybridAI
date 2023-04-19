using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace HybridAI.Control.Chat
{
    /// <summary>
    /// AnimationTextBox.xaml 的交互逻辑
    /// </summary>
    public partial class AnimationTextBox : UserControl
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(AnimationTextBox));
        private StringBuilder StringBuilder { get; } = new();

        public AnimationTextBox()
        {
            InitializeComponent();
        }

        public AnimationTextBox(string text) : this()
        {
            Text = text;
        }

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set
            {
                StringBuilder.Clear();
                StringBuilder.Append(value);
                SetValue(TextProperty, value);
                NormalTextBlock.Text = value;
            }
        }

        public async Task AddString(string character)
        {
            StringBuilder.Append(character);
            SetValue(TextProperty, StringBuilder.ToString());

            if (character == "\n")
            {
                if (Text[^2] == '\n')
                {
                    AnimationContainer.Children.Add(new TextBlock()
                    {
                        Width = double.MaxValue,
                        Text = ""
                    });
                }
                else
                {
                    AnimationContainer.Children.Add(new TextBlock()
                    {
                        Width = double.MaxValue,
                        Height = 0
                    });
                }

                return;
            }

            NormalTextBlock.Foreground = Foreground;

            var textBlock = new TextBlock()
            {
                Text = character,
                Opacity = 0,
                Effect = new BlurEffect()
                {
                    Radius = 20
                },
                RenderTransform = new ScaleTransform(),
                Foreground = Foreground,
                TextWrapping = TextWrapping.WrapWithOverflow
            };

            AnimationContainer.Children.Add(textBlock);

            MainWindow.PerformAppearAnimation(textBlock);

            var text = Text;
            await Task.Delay(150);

            textBlock.Visibility = Visibility.Hidden;
            NormalTextBlock.Text = text;
        }
    }
}
