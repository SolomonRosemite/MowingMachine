using System.Windows;

namespace MowingMachine
{
    /// <summary>
    /// Interaction logic for Popup.xaml
    /// </summary>
    public partial class Popup : Window
    {
        public Popup(string title, string description)
        {
            InitializeComponent();

            TitleLabel.Content = title;
            Description.Text = description;
        }

        private void OnButtonCloseClick(object sender, RoutedEventArgs e) =>
            this.Close();
    }
}
