using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace SOS.Core.Jobs
{
    public class MvmHarvestJob
    {
        public void Run()
        {
            Console.WriteLine($"Start harvesting MVM database: { DateTime.Now.ToLongTimeString() }");
            Thread.Sleep(30000);
            Console.WriteLine($"Finished harvesting MVM database: { DateTime.Now.ToLongTimeString() }");
        }
    }
}
