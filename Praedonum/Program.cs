using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using Praedonum.Modules;
using Praedonum.Observers;

namespace Praedonum
{
    class Program
    {
        private static IDictionary<string, PraedonumModule> _modules;
        private static IDictionary<string, IObserver> _observers;

        static void Main(string[] args)
        {
            _modules = RegisterModules();
            _observers = RegisterObservers(_modules.Values.ToList());

            //Create a server and listen on the presented prefixes
            HttpListener listener = new HttpListener();

            foreach (var module in _modules.Values)
            {
                listener.Prefixes.Add(String.Format("http://{0}:{1}/{2}/", GetLocalIpAddress(), 3389, module.Name));
            }

            listener.Start();

            Console.WriteLine("Praedonum is active!");
            Console.WriteLine("-------------------");
            Console.WriteLine();
            Console.WriteLine("Here are the list of modules that are available:");

            IList<string> moduleNames = _modules.Keys.ToList();
            for (int i = 0; i < moduleNames.Count; i++)
            {
                Console.WriteLine((i + 1) + ") " + moduleNames[i]);
            }

            Console.WriteLine();
            Console.WriteLine("-------------------");
            Console.WriteLine("Press ESC to stop");

            do
            {
                var result = listener.BeginGetContext(new AsyncCallback(ListenerCallback), listener);
                result.AsyncWaitHandle.WaitOne();
            } while (true);
        }

        private static IDictionary<string, PraedonumModule> RegisterModules()
        {
            IDictionary<string, PraedonumModule> modules = new ConcurrentDictionary<string, PraedonumModule>();

            foreach (Type type in
                Assembly.GetAssembly(typeof(PraedonumModule)).GetTypes()
                    .Where(
                        myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(PraedonumModule))))
            {
                modules[type.Name] = (PraedonumModule)Activator.CreateInstance(type);
            }

            return modules;
        }

        private static IDictionary<string, IObserver> RegisterObservers(IList<PraedonumModule> observables)
        {
            IDictionary<string, IObserver> observers = new ConcurrentDictionary<string, IObserver>();

            foreach (Type type in Assembly.GetAssembly(typeof(IObserver)).GetTypes().Where(myType => typeof(IObserver).IsAssignableFrom(myType) && !myType.IsInterface))
            {
                IObserver observer = (IObserver)Activator.CreateInstance(type);

                foreach (var observable in observables)
                {
                    if (observable is IObservable)
                    {
                        ((IObservable)observable).Attach(observer);
                    }
                }

                observers[type.Name] = observer;
            }

            return observers;
        }

        public static void ListenerCallback(IAsyncResult result)
        {
            HttpListener listener = (HttpListener)result.AsyncState;
            // Call EndGetContext to complete the asynchronous operation.
            HttpListenerContext context = listener.EndGetContext(result);

            if (_modules != null && _modules.Count > 0)
            {
                string moduleName = context.Request.Url.Segments.Length > 1 ? context.Request.Url.Segments[1] : "NONE";

                PraedonumModule module = null;
                _modules.TryGetValue(moduleName, out module);

                if (module != null)
                {
                    module.Execute(context.Request, context.Response);
                }
            }

            context.Response.Close();
        }

        public static string GetLocalIpAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());

            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }

            throw new Exception("Local IP Address Not Found!");
        }
    }
}
