using Lemon.HandyLib.Parallelization;

namespace Lemon.Toolkit.Test
{
    public class ParallelizationTest
    {
        [Fact]
        public void Test()
        {
            ParallelizationHelpler.MyParallelByThread(1, 160, i => 
            {
                Thread.Sleep(1000);
            });
        }
        [Fact]
        public async Task TestGpu()
        {
            await ParallelizationHelpler.SimulateGPULoad(100, 0.5F);
        }
    }
}