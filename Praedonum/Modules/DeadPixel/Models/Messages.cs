using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Praedonum.Modules.DeadPixel.Models
{
    public class Messages
    {
        public static string DeadPixelActivatedMessage = "Starting the DeadPixel module!";

        public static string DeadPixelDeactivatedMessage = "Ending the DeadPixel module!";

        public static string GetDeadPixelProgressMessage(Pixel pixel)
        {
            return $"Created a pixel at X:{pixel.X}, Y:{pixel.Y}, of size {pixel.Height}.";
        }
    }
}
