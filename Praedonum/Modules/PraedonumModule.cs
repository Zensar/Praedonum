using System;
using System.Net;
using System.Reflection;
using Praedonum.Modules.DeadPixel;

namespace Praedonum.Modules
{
    public abstract class PraedonumModule
    {
        public virtual string Name { get; }

        public abstract void Execute(HttpListenerRequest request, HttpListenerResponse response);
    }
}
