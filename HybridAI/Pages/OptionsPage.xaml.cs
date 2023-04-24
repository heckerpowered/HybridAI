using System.Windows;
using System.Windows.Controls;

namespace HybridAI.Pages
{
    /// <summary>
    /// OptionsPage.xaml 的交互逻辑
    /// </summary>
    public partial class OptionsPage : UserControl
    {

        public static readonly RoutedEvent GoBackEvent = EventManager.RegisterRoutedEvent("GoBack", RoutingStrategy.Direct, typeof(RoutedEventArgs), typeof(OptionsPage));
        public event RoutedEventHandler GoBack
        {
            add
            {
                AddHandler(GoBackEvent, value);
            }
            remove
            {
                RemoveHandler(GoBackEvent, value);
            }
        }
        public OptionsPage()
        {
            InitializeComponent();
        }

        private void Back(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(GoBackEvent, this));
        }
    }
}
