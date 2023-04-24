using System.Windows;
using System.Windows.Controls;

namespace HybridAI.Control.Chat
{
    /// <summary>
    /// WaitingResponseControl.xaml 的交互逻辑
    /// </summary>
    public partial class WaitingResponseControl : UserControl
    {
        public WaitingResponseControl(ChatContext context)
        {
            InitializeComponent();
            Context = context;
        }

        public ChatContext Context { get; }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            _ = Context.Interrupt();
        }
    }
}
