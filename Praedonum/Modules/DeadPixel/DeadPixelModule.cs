using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Timers;
using Praedonum.Modules.DeadPixel.Models;
using Praedonum.Observers;

namespace Praedonum.Modules.DeadPixel
{
    public class DeadPixelModule : PraedonumModule, IObservable
    {
        #region Fields

        private Timer _timer;
        private IList<Pixel> _pixels;
        private DateTime _start;

        private int _startX;
        private int _endX;
        private int _startY;
        private int _endY;
        private int _size;
        private int _speed;
        private int _last = 0;
        private Color _color = Color.Black;

        #endregion

        #region Properties

        public override string Name => typeof(DeadPixelModule).Name;

        public IList<IObserver> Observers { get; set; }

        #endregion

        #region Constructor / Destructor

        public DeadPixelModule()
        {
            _pixels = new List<Pixel>();
            Observers = new List<IObserver>();
        }

        #endregion

        #region Functions

        #region IObservable Implementation


        public void Attach(IObserver observer)
        {
            if (!Observers.Contains(observer))
            {
                Observers.Add(observer);
            }
        }

        public void Detach(IObserver observer)
        {
            if (Observers.Contains(observer))
            {
                Observers.Remove(observer);
            }
        }

        private void Notify(string message)
        {
            foreach (var observer in Observers)
            {
                observer.Update(message);
            }
        }

        #endregion

        public override void Execute(HttpListenerRequest request, HttpListenerResponse response)
        {
            bool start = false;
            bool.TryParse(request.QueryString["killPixels"], out start);

            bool clear = false;
            bool.TryParse(request.QueryString["clear"], out clear);

            int.TryParse(request.QueryString["startX"], out _startX);
            int.TryParse(request.QueryString["endX"], out _endX);
            int.TryParse(request.QueryString["startY"], out _startY);
            int.TryParse(request.QueryString["endY"], out _endY);
            int.TryParse(request.QueryString["size"], out _size);
            int.TryParse(request.QueryString["speed"], out _speed);
            _color = Color.FromName(request.QueryString["color"]);

            _endX = _endX > 0 ? _endX : 1920;
            _endY = _endY > 0 ? _endY : 1080;
            _size = _size > 0 ? _size : 2;
            _speed = _speed > 0 ? _speed : 10000;

            if (start)
            {
                if (_timer == null)
                {
                    Notify(Messages.DeadPixelActivatedMessage);
                    SetTimer();

                    var pixel = Pixel.CreateRandomPixel(_color, new Tuple<int, int>(_startX, _endX), new Tuple<int, int>(_startY, _endY), _size);

                    _pixels.Add(pixel);
                    Notify(Messages.GetDeadPixelProgressMessage(pixel));
                }
            }

            if (!start)
            {
                if (_timer != null)
                {
                    
                    Notify(Messages.DeadPixelDeactivatedMessage);
                    _timer.Stop();
                    _timer.Dispose();
                    _timer = null;
                    _last = 0;
                }

                if (clear)
                {
                    _pixels = new List<Pixel>();
                }
            }
        }

        private void SetTimer()
        {
            //Set initial timer start
            _start = DateTime.Now;

            // Create a timer with a two second interval.
            _timer = new System.Timers.Timer(1);

            // Hook up the Elapsed event for the timer. 
            _timer.Elapsed += OnTimedDrawEvent;
            _timer.AutoReset = true;
            _timer.Enabled = true;
        }


        /// <summary>
        /// Fires timed draw event
        /// </summary>
        /// <param name="source">Timer object</param>
        /// <param name="e">Elapsed time</param>
        private void OnTimedDrawEvent(Object source, ElapsedEventArgs e)
        {
            _timer?.Stop();

            AddRandomPixel(e.SignalTime);
            DrawRandomPixels();

            _timer?.Start();
        }

        /// <summary>
        /// Adds a new, random, pixel to the dead pixel collection
        /// </summary>
        /// <param name="signalTime">Elapsed time since starts</param>
        private void AddRandomPixel(DateTime signalTime)
        {
            TimeSpan timeSpan = (signalTime - _start);

            int interval = (int)Math.Floor((double)timeSpan.TotalMilliseconds / _speed);

            if (interval > _last)
            {
                var pixel = Pixel.CreateRandomPixel(_color, new Tuple<int, int>(_startX, _endX), new Tuple<int, int>(_startY, _endY), _size);
                _pixels.Add(pixel);
                Notify(Messages.GetDeadPixelProgressMessage(pixel));
                _last = interval;
            }
        }

        /// <summary>
        /// Draws collection of dead pixels to the screen (base of windows, overwrites all screens).
        /// </summary>
        private void DrawRandomPixels()
        {
            foreach (Pixel p in _pixels)
            {
                p.Draw();
            }
        }

        #endregion
    }
}
