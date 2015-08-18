using System;
using System.Collections.Generic;
using System.Threading;

namespace PlanetDevTest.Surface
{
    internal sealed class MeshGeneratorThread
    {
        private static int Ctr = 0;

        private static volatile bool Terminate = false;
        private static AutoResetEvent Event = new AutoResetEvent(false);
        private static Thread ExecutorThread;
        private static object SyncLock = new object();
		private static object QueueLock = new object ();
        private static Queue<Action> Queue = new Queue<Action>();

        private static void Executor()
        {
            while(!Terminate)
            {
                Event.WaitOne(10);

                while(true)
                {
					lock(QueueLock)
					{
						if(Queue.Count == 0)
							break;
						Queue.Dequeue()();
					}
                }
            }
        }
        private static void Create()
        {
            ExecutorThread = new Thread(new ThreadStart(Executor));
            ExecutorThread.Start();
        }
        public static void Dispose()
        {
            Terminate = true;
            Event.Set();
            ExecutorThread.Join();

            lock (QueueLock) 
			{
				while (Queue.Count != 0) 
				{
					Queue.Dequeue()();
				}
			}
        }

        internal static void Init()
        {
            if(Interlocked.Increment(ref Ctr) == 1)
            {
                lock (SyncLock)
                {
                    Create();
                }
            }
        }
        internal static void Deinit()
        {
            if(Interlocked.Decrement(ref Ctr) == 0)
            {
                lock (SyncLock)
                {
                    Dispose();
                }
            }
        }

        internal static void EnqueueTask(Action Task)
        {
			lock (QueueLock) 
			{
				Queue.Enqueue (Task);
			}
			Event.Set ();
        }
    }
}
