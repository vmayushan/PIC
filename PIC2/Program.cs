using System;
using System.Diagnostics;
using System.IO;

namespace PIC2
{
    class Program
    {
        static void Main(string[] args)
        {
            var time = new Stopwatch();
            time.Start();
            var pic = new ParticleInCell(0.015, 200, 1E-12/2, 3E-8, 12000, 0, ParticleMethod.CIC);
            pic.Run();
            pic.Report();
            time.Stop();
            Console.WriteLine("Завершено за {0} мс", time.ElapsedMilliseconds);
            Console.ReadLine();
        }
    }
}
