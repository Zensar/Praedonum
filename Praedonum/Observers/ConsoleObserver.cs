using System;

namespace Praedonum.Observers
{
    public class ConsoleObserver : IObserver
    {
        public void Update(string message)
        {
            Console.WriteLine(message);
        }
    }
}
