using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Lemon.HandyLib.Parallelization
{
    public static class ParallelizationHelpler
    {
        public static void MyParallelByThread(int inclusiveLowerBound, 
            int exclusiveUpperBound, 
            Action<int> body)
        {
            // Determine the number of iterations to be processed, the number of
            // cores to use, and the approximate number of iterations to process
            // in each thread.
            int size = exclusiveUpperBound - inclusiveLowerBound;
            int numProcs = Environment.ProcessorCount;
            int range = size / numProcs;
            // Use a thread for each partition. Create them all,
            // start them all, wait on them all.
            var threads = new List<Thread>(numProcs);
            for (int p = 0; p < numProcs; p++)
            {
                int start = p * range + inclusiveLowerBound;
                int end = (p == numProcs - 1) ?
                exclusiveUpperBound : start + range;
                threads.Add(new Thread(() =>
                {
                    for (int i = start; i < end; i++) body(i);
                }));
            }
            foreach (var thread in threads) thread.Start();
            foreach (var thread in threads) thread.Join();
        }

        public static void MyParallelByThreadPool(int inclusiveLowerBound, int exclusiveUpperBound, Action<int> body)
        {
            // Determine the number of iterations to be processed, the number of
            // cores to use, and the approximate number of iterations to process in
            // each thread.
            int size = exclusiveUpperBound - inclusiveLowerBound;
            int numProcs = Environment.ProcessorCount;
            int range = size / numProcs;
            // Keep track of the number of threads remaining to complete.
            int remaining = numProcs;
            using ManualResetEvent mre = new(false);
            // Create each of the threads.
            for (int p = 0; p < numProcs; p++)
            {
                int start = p * range + inclusiveLowerBound;
                int end = (p == numProcs - 1) ?
                exclusiveUpperBound : start + range;
                ThreadPool.QueueUserWorkItem(delegate
                {
                    for (int i = start; i < end; i++) body(i);
                    if (Interlocked.Decrement(ref remaining) == 0) mre.Set();
                });
            }
            // Wait for all threads to complete.
            mre.WaitOne();
        }
        public static async Task SimulateGPULoad(int durationSeconds, float loadPercentage)
        {
            using var cancellationTokenSource = new CancellationTokenSource();
            var token = cancellationTokenSource.Token;

            var simulationTask = Task.Run(() =>
            {
                var watch = new Stopwatch();
                var random = new Random();

                using var surface = SKSurface.Create(new SKImageInfo(1000, 1000));
                var canvas = surface.Canvas;

                DateTime startTime = DateTime.Now;
                while (!token.IsCancellationRequested &&
                       (DateTime.Now - startTime).TotalSeconds < durationSeconds)
                {
                    watch.Restart();

                    // 复杂的图形渲染操作模拟计算负载
                    while (watch.ElapsedMilliseconds < loadPercentage)
                    {
                        canvas.Clear(SKColors.White);

                        using var paint = new SKPaint
                        {
                            Color = SKColors.Blue,
                            Style = SKPaintStyle.Fill
                        };

                        // 随机复杂图形绘制
                        for (int i = 0; i < 100; i++)
                        {
                            float x = (float)random.NextDouble() * 1000;
                            float y = (float)random.NextDouble() * 1000;
                            float radius = (float)random.NextDouble() * 50;
                            canvas.DrawCircle(x, y, radius, paint);
                        }

                        // 模拟图像处理
                        var image = surface.Snapshot();
                        var bitmap = SKBitmap.FromImage(image);
                        bitmap.Dispose();
                    }

                    // 控制总体负载
                    Thread.Sleep((int)((100 - loadPercentage) / 100.0 * 100));
                }
            }, token);

            await Task.Delay(TimeSpan.FromSeconds(durationSeconds), token);
            cancellationTokenSource.Cancel();
            await simulationTask;
        }


    }
}
