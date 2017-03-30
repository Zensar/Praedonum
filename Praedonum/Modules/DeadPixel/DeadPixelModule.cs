using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Timers;
using Praedonum.Modules.DeadPixel.Models;

namespace Praedonum.Modules.DeadPixel
{
    public class DeadPixelModule : PraedonumModule
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

        #endregion

        public DeadPixelModule()
        {
            _pixels = new List<Pixel>();
        }

        public override string Name => typeof(DeadPixelModule).Name; 

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

            _endX = _endX > 0 ? _endX : 1920;
            _endY = _endY > 0 ? _endY : 1080;
            _size = _size > 0 ? _size : 2;
            _speed = _speed > 0 ? _speed : 10000;

            if (start)
            {
                if (_timer == null)
                {
                    Console.WriteLine();
                    Console.WriteLine("Starting dead pixel!");
                    SetTimer();
                    _pixels.Add(Pixel.CreateRandomPixel(new Tuple<int, int>(_startX, _endX), new Tuple<int, int>(_startY, _endY), _size));
                }
            }

            if (!start)
            {
                if (_timer != null)
                {
                    Console.WriteLine();
                    Console.WriteLine("Ending dead pixel!");
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
            _timer.Stop();

            AddRandomPixel(e.SignalTime);
            DrawRandomPixels();

            _timer.Start();
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
                _pixels.Add(Pixel.CreateRandomPixel(new Tuple<int, int>(_startX, _endX), new Tuple<int, int>(_startY, _endY), _size));
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
    }
}
