using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Praedonum.Modules;

namespace Praedonum
{
    class Program
    {
        private static IDictionary<string, PraedonumModule> _modules;

        static void Main(string[] args)
        {
            var prefix = BuildRootPrefix(args);
            _modules = RegisterModules();

            HttpListener listener = new HttpListener();

            foreach (var module in _modules.Values)
            {
                listener.Prefixes.Add(String.Format("{0}{1}/", prefix, module.Name));
            }

            listener.Start();

            Console.WriteLine("Praedonum is active through - '" + prefix + "{moduleName}'");
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
                while (!Console.KeyAvailable)
                {
                    var result = listener.BeginGetContext(new AsyncCallback(ListenerCallback), listener);
                    result.AsyncWaitHandle.WaitOne();
                }
            } while (Console.ReadKey(true).Key != ConsoleKey.Escape);
        }

        private static IDictionary<string, PraedonumModule> RegisterModules()
        {
            IDictionary<string, PraedonumModule> modules = new ConcurrentDictionary<string, PraedonumModule>();

            foreach (Type type in
                Assembly.GetAssembly(typeof(PraedonumModule)).GetTypes()
                    .Where(
                        myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(PraedonumModule))))
            {
                modules[type.Name] = (PraedonumModule) Activator.CreateInstance(type);
            }

            return modules;
        }

        public static void ListenerCallback(IAsyncResult result)
        {
            HttpListener listener = (HttpListener) result.AsyncState;
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

        public static Prefix BuildRootPrefix(string[] args)
        {
            var prefix = new Prefix();
            prefix.Protocol = "http";
            prefix.Host = GetLocalIPAddress();
            prefix.Port = 3389;

            return prefix;
        }

        public static string GetLocalIPAddress()
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
