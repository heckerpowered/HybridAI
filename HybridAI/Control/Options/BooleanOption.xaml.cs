using System.Windows;
using System.Windows.Controls;

namespace HybridAI.Control.Options
{
    /// <summary>
    /// BooleanOption.xaml 的交互逻辑
    /// </summary>
    public partial class BooleanOption : OptionControl
    {
        public BooleanOption()
        {
            InitializeComponent();
        }

        public delegate void BooleanPropertyChangedEvent(bool Value);
        public event BooleanPropertyChangedEvent? BooleanPropertyChanged;
        public bool Value
        {
            get => CheckBox.IsChecked ?? false;
            set => CheckBox.IsChecked = value;
        }

        private void ClickCheckBox(object sender, RoutedEventArgs e)
        {
            BooleanPropertyChanged?.Invoke(Value);
        }
    }
}
