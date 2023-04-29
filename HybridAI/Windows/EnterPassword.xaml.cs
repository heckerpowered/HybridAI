using System.Windows;

namespace HybridAI.Windows
{
    /// <summary>
    /// EnterPassword.xaml 的交互逻辑
    /// </summary>
    public partial class EnterPassword : Window
    {
        public EnterPassword()
        {
            InitializeComponent();
        }

        public bool Confirmed { get; private set; }

        private void Canceled(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Confirm(object sender, RoutedEventArgs e)
        {
            Confirmed = true;
            Close();
        }
    }
}
