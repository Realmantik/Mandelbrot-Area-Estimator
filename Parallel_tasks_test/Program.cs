using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

internal class MandelbrotAreaEstimator
{
    private static int MAX_ITER = 40000;
    private static readonly int MAX_MAG_SQUARED = 4;
    private static double square;
    private static int factor;
    private static readonly double Xmax = 0.47;
    private static readonly double Xmin = -2;
    private static readonly double Ymax = 1.12;
    private static readonly double Ymin = 0;
    private static int Height = 1120*20;
    private static int Width = 2470*20;
    private static int Counter = 0;
    private static int percent = 0;

    private static void Main()
    {
        var set_max_iter = false;
        var factor_is_set = false;
        do
        {
            Console.WriteLine("Введите количество итераций на точку:");
            var s1 = Console.ReadLine();
            if (int.TryParse(s1, out MAX_ITER)) set_max_iter = true;
        } while (!set_max_iter);

        do
        {
            Console.WriteLine("Введите множитель дробления(целое число):");
            var s1 = Console.ReadLine();
            if (int.TryParse(s1, out factor)){ factor_is_set = true;
                Height = 1120 * factor;
                Width = 2470 * factor;
            }
        } while (!factor_is_set);

        percent = Width / 100;
        var watch = new Stopwatch();
        var monitor = new object();
        watch.Start();
        var r= Task.Factory.StartNew(() => Parallel.For(0, Width, new ParallelOptions {MaxDegreeOfParallelism = Environment.ProcessorCount}, () => 0.0,
            (i, state, local) => { return local + Go(i); },
            local =>
            {
                lock (monitor)
                {
                    square += local;
                }
            }), TaskCreationOptions.LongRunning).Result;
        watch.Stop();
        Console.WriteLine("Площадь множества Мандельброта: "+2 * (2.47 * 1.12) *square);
        Console.WriteLine(watch.Elapsed);
        Console.ReadLine();
    }

    private static double Go(int i_x_coordinate)
    {
        Interlocked.Increment(ref Counter);

        int iter = 0;
        var local_sum = 0;
        var dReaC = (Xmax - Xmin) / (Width - 1);
        var dImaC = (Ymax - Ymin) / (Height - 1);
        var ReaC = Xmin + i_x_coordinate * dReaC;

        var ImaC = Ymin;
        for (var y = 0; y < Height; y++)
        {
            iter++;
            double ReaZ = 0;
            double ImaZ = 0;
            double ReaZ2 = 0;
            double ImaZ2 = 0;
            var clr = 1;
            if (Math.Sqrt((ReaC - 0.25) * (ReaC - 0.25) + ImaC * ImaC) <
                0.5 - 0.5 * Math.Cos(Math.Atan2(ImaC, ReaC - 0.25)))
            {
                local_sum++;
                ImaC += dImaC;
                continue;
            }

            if (Math.Sqrt((ReaC + 1) * (ReaC + 1) + ImaC * ImaC) < 0.25)
            {
                local_sum++;
                ImaC += dImaC;
                continue;
            }
            if (Math.Sqrt((ReaC+0.12485)*(ReaC+0.12485)+(ImaC-0.744)*(ImaC-0.744))<0.09439)
            {
                local_sum++;
                ImaC += dImaC;
                continue;
            }
            if (Math.Sqrt((ReaC+1.30904)*(ReaC+1.30904)+(ImaC)*(ImaC))<0.05895)
            {
                local_sum++;
                ImaC += dImaC;
                continue;
            }
            while (clr < MAX_ITER && ReaZ2 + ImaZ2 < MAX_MAG_SQUARED)
            {
                ReaZ2 = ReaZ * ReaZ;
                ImaZ2 = ImaZ * ImaZ;
                ImaZ = 2 * ImaZ * ReaZ + ImaC;
                ReaZ = ReaZ2 - ImaZ2 + ReaC;
                clr++;
            }

            if (ReaZ2 + ImaZ2 < MAX_MAG_SQUARED) local_sum++;
            ImaC += dImaC;
        }
        
        if (Counter%percent==0)
        {
            Console.Clear();
            Console.WriteLine(100*(double)Counter/(double)Width+"%");
        }
        return ((double)local_sum / (double)iter)/(Width);
    }
}