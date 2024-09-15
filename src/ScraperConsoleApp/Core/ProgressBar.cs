using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ScraperConsoleApp.Core
{
    public class ProgressBar
    {
         public void DisplayProgress(int progress, int total)
        {
            int width = 50; 
            float percent = (float)progress / total;
            int progressWidth = (int)(width * percent);

            Console.CursorLeft = 0;
            Console.Write("[");
            Console.Write(new string('#', progressWidth));
            Console.Write(new string(' ', width - progressWidth));
            Console.Write($"] {progress}/{total} ({percent * 100:0.0}%)");
        }
    }
}