using System;
using System.ComponentModel;

namespace Utils
{
    public class StatusProgress : INotifyPropertyChanged, IDisposable
    {
        public const string INFO = "Info";
        public const string ERROR = "Error";

        public static uint GlobalStatusId = 0;
        private static object GlobalStatusIdBlocker = new object();
        public readonly uint statusId = 0;
        public readonly uint previousStatusId = 0;
        public int endRate = 100;
        public bool isPercentage;

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public StatusProgress(int? rate = null, bool isPercentage = true)
        {
            lock (GlobalStatusIdBlocker)
            {
                statusId = GlobalStatusId;

                if (GlobalStatusId == uint.MaxValue)
                    GlobalStatusId = 1;
                else
                    GlobalStatusId++;
            }

            this.isPercentage = isPercentage;
            Rate = rate;
        }

        public StatusProgress(string message, int? rate = null, bool isPercentage = true)
        {
            lock (GlobalStatusIdBlocker)
            {
                statusId = GlobalStatusId;

                if (GlobalStatusId == uint.MaxValue)
                    GlobalStatusId = 1;
                else
                    GlobalStatusId++;
            }

            this.isPercentage = isPercentage;
            Message = message;
            Rate = rate;
        }

        public StatusProgress(string message, uint previousStatusId, int? rate = null) : this(message, rate)
        {
            lock (GlobalStatusIdBlocker)
            {
                if (GlobalStatusId < previousStatusId)
                    statusId = previousStatusId;
                else
                    throw new Exception("Não é assim que usa");
            }
        }

        private string prefix;
        public string Prefix
        {
            get { return prefix; }
            set { prefix = value; }
        }

        private string message;
        public string Message
        {
            get { return message; }
            set { message = Prefix == null ? value : Prefix + message; NotifyPropertyChanged(nameof(Message)); }
        }

        private int? rate;
        public int? Rate
        {
            get { return rate; }
            set { rate = value; NotifyPropertyChanged(nameof(Rate)); }
        }

        public string GetFormattedStatus()
        {
            if (Rate.HasValue)
            {
                return isPercentage
                    ? $"{Message} {Rate}%"
                    : $"{Message} {Rate}/{endRate}";
            }
            return Message;
        }

        public void ChangeMessageAndRate(string message, int? rate)
        {
            this.message = message;
            Rate = rate;
        }

        object rateLocker = new object();
        /// <summary>
        /// Thread safe method
        /// Don't call this method in the main thread, since, if you're doing this, something is wrong(Will cause a threadlock)
        /// </summary>
        /// <param name="value"></param>
        public void InterlockedIncrementRate(int value = 1)
        {
            lock (rateLocker)
                Rate += value;
        }

        public void DefineAsComplete()
        {
            this.Rate = endRate;
        }

        #region Hash, ToString, IEquatable, IComparable

        public override int GetHashCode()
        {
            return statusId.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format(System.Globalization.CultureInfo.InvariantCulture,
                @"{0}={{StatusId:{1},Message:{2},Rate:{3}}}",
                typeof(StatusProgress).Name, statusId, Message, Rate);
        }

        public void Dispose()
        {
            this.DefineAsComplete();
        }

        #endregion
    }
}