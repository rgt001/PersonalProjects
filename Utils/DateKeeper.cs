using System;
using System.Threading;

namespace Utils
{
    public static class DateKeeper
    {
        private static readonly object _lock = new object();
        private static DateTime _date = DateTime.Today;

        // This property is important because it prevents external modifications to the date managed by this class
        public static DateTime Date => _date;

        static DateKeeper()
        {
            // Initializes the timer to update the date at midnight
            Timer timer = new Timer(UpdateDate, null, CalculateInitialDelay(), TimeSpan.FromDays(1));
        }

        private static void UpdateDate(object state)
        {
            lock (_lock)
            {
                _date = DateTime.Today; // Safely updates the date
            }
        }

        // Calculates the initial delay until midnight
        private static TimeSpan CalculateInitialDelay()
        {
            var now = DateTime.Now;
            var tomorrow = DateTime.Today.AddDays(1);
            return tomorrow - now; // Time remaining until midnight
        }
    }
}
