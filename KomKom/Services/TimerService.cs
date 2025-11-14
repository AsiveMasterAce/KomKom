using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace KomKom.Services
{
    public class TimerService
    {
        private readonly DispatcherTimer _timer;
        public TimeSpan TimeLeft { get; private set; } = TimeSpan.Zero;
        public bool IsRunning => _timer.IsEnabled;


        public event Action<TimeSpan> Tick;
        public event Action Finished;

        public TimerService()
        {
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += (s, e) =>
            {
                TimeLeft = TimeLeft - TimeSpan.FromSeconds(1);
                Tick?.Invoke(TimeLeft);
                if (TimeLeft <= TimeSpan.Zero)
                {
                    _timer.Stop();
                    Finished?.Invoke();
                }
            };
        }


        public void Start(TimeSpan duration)
        {
            TimeLeft = duration;
            _timer.Start();
        }


        public void Pause() => _timer.Stop();
        public void Resume() => _timer.Start();
        public void Reset() => _timer.Stop();
    }
}
