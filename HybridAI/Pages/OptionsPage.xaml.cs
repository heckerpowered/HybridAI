using System.Windows;
using System.Windows.Controls;

using HybridAI.Control.Options;
using HybridAI.Language;
using HybridAI.Options;

namespace HybridAI.Pages
{
    /// <summary>
    /// OptionsPage.xaml 的交互逻辑
    /// </summary>
    public partial class OptionsPage : UserControl
    {

        public static readonly RoutedEvent GoBackEvent = EventManager.RegisterRoutedEvent("GoBack", RoutingStrategy.Direct, typeof(RoutedEventArgs), typeof(OptionsPage));
        public OptionsPage()
        {
            InitializeComponent();

            ComboOption.Combo.SelectionChanged += ChangeLanguage;

            foreach (var languageDisplayName in Languages.LanguageDisplayNames)
            {
                ComboOption.Combo.Items.Add(languageDisplayName);
            }

            ComboOption.Combo.SelectedIndex = Properties.Settings.LanguageIndex;
        }
        public delegate void LanguageChangedEvent(int LanguageIndex);
        public event LanguageChangedEvent? LanguageChanged;
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
        public event BooleanOption.BooleanPropertyChangedEvent? ExplicitEncryptionPropertyChanged;

        private void ChangeLanguage(object sender, SelectionChangedEventArgs e)
        {
            var languageIndex = ComboOption.Combo.SelectedIndex;
            LanguageChanged?.Invoke(languageIndex);
            Properties.Settings.LanguageIndex = languageIndex;
        }

        private void Back(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(GoBackEvent, this));
        }

        private void ExplicitEncryptionChanged(bool Value)
        {
            ExplicitEncryptionPropertyChanged?.Invoke(Value);
        }
    }
}
