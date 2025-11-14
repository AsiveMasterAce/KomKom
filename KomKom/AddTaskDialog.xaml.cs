using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace KomKom
{
    /// <summary>
    /// Interaction logic for AddTaskDialog.xaml
    /// </summary>
    public partial class AddTaskDialog : Window
    {
        public string TaskTitle { get; private set; }
        public DateTime TaskStartTime { get; private set; }
        private DateTime? startTime;
        public int TaskDuration { get; private set; }
        public event PropertyChangedEventHandler PropertyChanged;

        public AddTaskDialog()
        {
            InitializeComponent();

            // Set default date to today
            //StartDatePicker.SelectedDate = DateTime.Today;
            DataContext = this;
            StartTime = DateTime.Today;
        }

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));


        public DateTime? StartTime
        {
            get => startTime;
            set
            {
                startTime = value;
                OnPropertyChanged(nameof(StartTime));
            }
        }
        private void AddTask_Click(object sender, RoutedEventArgs e)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
            {
                MessageBox.Show("Please enter a task title.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(HourTextBox.Text, out int hour) || hour < 0 || hour > 23)
            {
                MessageBox.Show("Please enter a valid hour (0-23).", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(MinuteTextBox.Text, out int minute) || minute < 0 || minute > 59)
            {
                MessageBox.Show("Please enter a valid minute (0-59).", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(DurationTextBox.Text, out int duration) || duration <= 0)
            {
                MessageBox.Show("Please enter a valid duration (greater than 0).", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (StartDatePicker.SelectedDate == null)
            {
                MessageBox.Show("Please select a start date.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Set properties
            TaskTitle = TitleTextBox.Text.Trim();
            TaskStartTime = StartDatePicker.SelectedDate.Value.Date
                .AddHours(hour)
                .AddMinutes(minute);
            TaskDuration = duration;

            // Close dialog with success
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
