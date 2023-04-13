﻿using System.Text;
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
                NormalTextBox.Text = value;
            }
        }

        public async void AddString(string character)
        {
            StringBuilder.Append(character);
            SetValue(TextProperty, StringBuilder.ToString());

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

            MainWindow.PlayAppearAnimation(textBlock);

            await Task.Delay(150);
        }
    }
}