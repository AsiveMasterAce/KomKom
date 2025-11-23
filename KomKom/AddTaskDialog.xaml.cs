using System;
using System.ComponentModel;
using System.Windows;

namespace KomKom
{
    public partial class AddTaskDialog : Window, INotifyPropertyChanged
    {
        public string TaskTitle { get; private set; }
        public DateTime TaskStartTime { get; private set; }
        public int TaskDuration { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

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

            if (!int.TryParse(DurationTextBox.Text, out int duration) || duration <= 0)
            {
                MessageBox.Show("Please enter a valid duration.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (StartDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Please select a start date.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!(HourComboBox.SelectedItem is int hour) || !(MinuteComboBox.SelectedItem is int minute))
            {
                MessageBox.Show("Please select a valid start time.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Set properties
            TaskTitle = TitleTextBox.Text.Trim();
            TaskStartTime = StartDatePicker.SelectedDate.Value.Date
                .AddHours(hour)
                .AddMinutes(minute);
            TaskDuration = duration;

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
