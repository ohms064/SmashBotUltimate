using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SmashBotUltimate.Bot.Modules.SavedDataServices;

namespace SmashBotUltimate.Bot.Modules.SavedDataServices {

    public class TimerData {
        public event Action callback;
        public TimeSpan timeSpan;
        public string description;

        private CancellationTokenSource _timerCancel;

        public TimerData () {
            _timerCancel = new CancellationTokenSource ();
        }

        public void Start () {
            Task t = Timer ();
        }

        public void Cancel () {
            _timerCancel.Cancel ();
        }

        private async Task Timer () {
            try {
                await Task.Delay (timeSpan, _timerCancel.Token);
            } catch (TaskCanceledException) {
                return;
            }
            callback?.Invoke ();
        }
    }

    public class TimerService : ISavedData<string, TimerData> {
        private Dictionary<string, TimerData> _timers;

        public TimerService () {
            _timers = new Dictionary<string, TimerData> ();
        }
        public bool HasData (string key) {
            return _timers.ContainsKey (key);
        }
        public void SaveData (string key, TimerData data) {
            if (_timers.ContainsKey (key)) RemoveData (key);
            data.callback += () => {
                if (_timers.ContainsKey (key))
                    _timers.Remove (key);
            };
            _timers.Add (key, data);
            data.Start ();
        }
        public void RemoveData (string key) {
            if (!_timers.ContainsKey (key)) return;
            _timers[key].Cancel ();
            _timers.Remove (key);
        }

        public bool TryGetData (string key, out TimerData value) {
            value = null; //We dont really save TimerData.
            return false;
        }
    }
}