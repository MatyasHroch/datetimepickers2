using System;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        bool appReady = false;

        // to be able to modify if one component should update its dependecies we have this bool Flag
        bool updateDependencies = false;

        // to be able to tell if we should move with the start or the end datetime
        bool changingStartDateTime = false;

        DateTime currentDateTime;
        DateTime startDateTime;
        DateTime endDateTime;

        DispatcherTimer timer;

        public MainWindow()
        {
            // setting up datetimes
            var appStart = DateTime.Now;
            this.currentDateTime = DateTime.Now;
            this.startDateTime = appStart.AddMinutes(0).AddSeconds(- appStart.Second).AddMilliseconds(- appStart.Millisecond);
            this.endDateTime = appStart.AddMinutes(1).AddSeconds(-appStart.Second).AddMilliseconds(-appStart.Millisecond);

            InitializeComponent();

            // current Datetime Text Field
            currentDatetimeText.Text = this.currentDateTime.ToString();

            // setting up slider
            slider.Maximum = 100;
            slider.Minimum = 0;
            
            // Updating the controls acording to the data
            UpdateAllPickers(this.startDateTime, this.endDateTime);
            UpdateSlider(this.currentDateTime, this.startDateTime, this.endDateTime);

            // setting the app ready and 
            this.updateDependencies = true;
            this.changingStartDateTime = this.changingStartDateTime || startDateTime > currentDateTime;
            this.appReady = true;

            // development
            //changingStartText.Text = "Changing the Start: " + this.changingStartDateTime.ToString();
            changingStartText.Text = "";

            // Initialize the timer to update current time every second
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        // this method will be called every second
        private void Timer_Tick(object? sender, EventArgs? e)
        {
            this.currentDateTime = DateTime.Now;
            currentDatetimeText.Text = this.currentDateTime.ToString();

            bool updateDependenciesValue = this.updateDependencies;
            this.updateDependencies = false;
            UpdateSlider(this.currentDateTime, this.startDateTime, this.endDateTime);
            this.updateDependencies = updateDependenciesValue;
        }

        //
        private void StartDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!appReady) return;
            UpdateStartDateTimeFromPicker();

            if (this.updateDependencies)
            {
                this.updateDependencies = false;
                UpdateSlider(this.currentDateTime, this.startDateTime, this.endDateTime);
                this.updateDependencies = true;
            }
        }

        private void StartTimePicker_ValueChanged(object sender, RoutedPropertyChangedEventArgs<DateTime?> e)
        {
            if (!appReady) return;
            UpdateStartDateTimeFromPicker();

            if (this.updateDependencies)
            {
                this.updateDependencies = false;
                UpdateSlider(this.currentDateTime, this.startDateTime, this.endDateTime);
                this.updateDependencies = true;
            }
        }

        private void EndDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!appReady) return;
            UpdateEndDateTimeFromPicker();

            if (updateDependencies)
            {
                this.updateDependencies = false;
                UpdateSlider(this.currentDateTime, this.startDateTime, this.endDateTime);
                this.updateDependencies = true;
            }
        }

        private void EndTimePicker_ValueChanged(object sender, RoutedPropertyChangedEventArgs<DateTime?> e)
        {
            if (!appReady) return;
            UpdateEndDateTimeFromPicker();

            if (updateDependencies)
            {
                this.updateDependencies = false;
                UpdateSlider(this.currentDateTime, this.startDateTime, this.endDateTime);
                this.updateDependencies = true;
            }
        }

        private void Slider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            this.changingStartDateTime = false;
        }

        // updates all pickers to the values of all DateTimes, with no dependecy trigger
        private void UpdateAllPickers(DateTime startDateTime, DateTime endDateTime)
        {
            bool updateDependenciesValue = this.updateDependencies;
            this.updateDependencies = false;

            startDatePicker.SelectedDate = startDateTime;
            startTimePicker.SelectedTime = NewDateTime(startDateTime);
            endDatePicker.SelectedDate = endDateTime;
            endTimePicker.SelectedTime = endDateTime;

            this.updateDependencies = updateDependenciesValue;
        }

        private void Slider_ValueChanged(object sender, RoutedEventArgs e)
        {
            if (!appReady) return;

            this.changingStartDateTime = this.changingStartDateTime || startDateTime > currentDateTime;

            double percents = slider.Value;
            currentPercents.Text = percents.ToString() + " %";

            if (!this.updateDependencies) return;

            //TimeSpan fullTimeRange = endDateTime - startDateTime;
            TimeSpan beforeTimeRange = this.currentDateTime - startDateTime;
            TimeSpan afterTimeRange = this.endDateTime - currentDateTime; 

            var fromCurrentToEndPercents = 100 - percents;

            if (fromCurrentToEndPercents == 0 || percents == 0)
            {
                // Handle this edge case
                return;
            }

            if (this.changingStartDateTime)
            {
                var onePercent = afterTimeRange / fromCurrentToEndPercents;
                this.startDateTime = this.currentDateTime - (onePercent * percents);
            }
            else
            {
                var onePercent =  beforeTimeRange / percents;
                this.endDateTime = currentDateTime + (onePercent * fromCurrentToEndPercents);
            }

            UpdateAllPickers(this.startDateTime, this.endDateTime);
        }

        private void UpdateStartDateTimeFromPicker()
        {
            DateTime? startDate = startDatePicker.SelectedDate;
            DateTime? startTime = startTimePicker.SelectedTime;

            if (startDate.HasValue && startTime.HasValue)
            {
                var startDateTimeValue = CombineDateAndTime(startDate.Value, startTime.Value);

                // Add a safeguard: Check if startDateTimeValue is beyond endDateTime
                if (startDateTimeValue > this.endDateTime)
                {
                    startDateTimeValue = this.endDateTime.AddSeconds(-1); 
                }

                // Assign corrected startDateTime
                this.startDateTime = startDateTimeValue;

                // Flag if the start is after current time, set the option to change the startDateTime
                this.changingStartDateTime = this.changingStartDateTime || this.startDateTime > this.currentDateTime;

                // just for the development 
                //changingStartText.Text = "Changing the Start: " + this.changingStartDateTime.ToString();
                changingStartText.Text = "";

                // Update the UI based on the new start time
                UpdateAllPickers(this.startDateTime, this.endDateTime);
                UpdateSlider(this.currentDateTime, this.startDateTime, this.endDateTime);
            }
        }



        private void UpdateEndDateTimeFromPicker()
        {
            DateTime? endDate = endDatePicker.SelectedDate;
            DateTime? endTime = endTimePicker.SelectedTime;

            if (endDate.HasValue && endTime.HasValue)
            {
                var endDateTimeValue = CombineDateAndTime(endDate.Value, endTime.Value);

            if (endDateTimeValue < this.startDateTime)
                {
                    endDateTimeValue = this.startDateTime.AddSeconds(1);
                }

                // Assign corrected endDateTime
                this.endDateTime = endDateTimeValue;

                // Update the UI based on the new end time
                UpdateAllPickers(this.startDateTime, this.endDateTime);
                UpdateSlider(this.currentDateTime, this.startDateTime, this.endDateTime);
            }
        }


        private void UpdateSlider(DateTime currentDateTime, DateTime startDateTime, DateTime endDateTime)
        {
            var newSliderValue = PastTimeInPercents(currentDateTime, startDateTime, endDateTime);

            // we remember value of the update
            var updateDependencies = this.updateDependencies;
            if (newSliderValue is not double.NaN && !newSliderValue.Equals(double.PositiveInfinity))
            {
                this.updateDependencies = false;
                slider.Value = newSliderValue;
                this.updateDependencies = updateDependencies;
            }
        }

        static DateTime CombineDateAndTime(DateTime date, DateTime time)
        {
            int year = date.Year;
            int month = date.Month;
            int day = date.Day;

            int hour = time.Hour;
            int minute = time.Minute;
            int second = time.Second;
            int millisecond = time.Millisecond;

            return new DateTime(year, month, day, hour, minute, second, millisecond);
        }

        // Rounds the result to two decimal points
        static double PastTimeInPercents(DateTime currentTime, DateTime startDateTime, DateTime endDateTime)
        {
            TimeSpan fullTimeRange = endDateTime - startDateTime;
            TimeSpan pastTimeRange = currentTime - startDateTime;

            // If fullTimeRange is zero or negative, return 100 (or 0 depending on the direction)
            if (fullTimeRange.Ticks <= 0)
            {
                // Maximum value
                return 100;
            }

            double result = pastTimeRange.Ticks / (fullTimeRange.Ticks / 100.0);

            // Clamp the result between 0 and 100 to ensure it never exceeds the bounds
            return Math.Max(0, Math.Min(Math.Round(result, 2), 100));
        }


        static DateTime NewDateTime(
            DateTime originalDateTime,
            int? year = null,
            int? month = null,
            int? day = null,
            int? hour = null,
            int? minute = null,
            int? second = null)
        {
            return new DateTime(
                year ?? originalDateTime.Year,
                month ?? originalDateTime.Month,
                day ?? originalDateTime.Day,
                hour ?? originalDateTime.Hour,
                minute ?? originalDateTime.Minute,
                second ?? originalDateTime.Second
            );
        }
    }
}
