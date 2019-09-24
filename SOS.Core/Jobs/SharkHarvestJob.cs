using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SOS.Core.Jobs
{
    public class SharkHarvestJob
    {
        public void Run()
        {
            Console.WriteLine($"Start harvesting SHARK database: { DateTime.Now.ToLongTimeString() }");
            Thread.Sleep(30000);
            Console.WriteLine($"Finished harvesting SHARK database: { DateTime.Now.ToLongTimeString() }");
        }
    }
}
