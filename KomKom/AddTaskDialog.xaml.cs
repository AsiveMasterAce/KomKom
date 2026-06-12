using System;
using System.ComponentModel;
using System.Windows;

namespace KomKom
{
    public partial class AddTaskDialog : Window, INotifyPropertyChanged
    {
        public string TaskTitle { get; private set; } = string.Empty;
        public int TaskPriority { get; private set; }
        public string TaskTags { get; private set; } = string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;

        public AddTaskDialog()
        {
            InitializeComponent();
            DataContext = this;
        }

        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            // Validation
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
            {
                MessageBox.Show("Please enter a task title.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!(Priority1Radio.IsChecked == true || Priority2Radio.IsChecked == true || Priority3Radio.IsChecked == true || Priority4Radio.IsChecked == true))
            {
                MessageBox.Show("Please choose a priority.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (Priority1Radio.IsChecked == true)
            {
                TaskPriority = 1;
            }
            else if (Priority2Radio.IsChecked == true)
            {
                TaskPriority = 2;
            }
            else if (Priority3Radio.IsChecked == true)
            {
                TaskPriority = 3;
            }
            else
            {
                TaskPriority = 4;
            }

            TaskTitle = TitleTextBox.Text.Trim();
            TaskTags = TagsTextBox.Text.Trim();

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
