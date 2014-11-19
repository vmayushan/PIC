using System;
using System.Diagnostics;
using System.IO;

namespace PIC2
{
    class Program
    {
        static void Main(string[] args)
        {

            //var pic = new ParticleInCell(0.01, 101, 1E-12, 5E-8, 1000, 1);
            //var pic = new ParticleInCell(0.15, 124, 1E-12, 1E-8, 120000, 1);

            var time = new Stopwatch();
            time.Start();
            var pic = new ParticleInCell(0.015, 200, 1E-11, 2E-8, 12000, 0, ParticleMethod.CIC);
            pic.Run();
            pic.Report();
            time.Stop();
            Console.WriteLine("Завершено за {0} мс", time.ElapsedMilliseconds);



            Console.ReadLine();
        }
    }
}
