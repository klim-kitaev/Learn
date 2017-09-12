using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PP
{
    class Program
    {
        static void Main(string[] args)
        {
            DictionaryTest();
        }


        static void AccountTest()
        {
            var account = new BankAcount();


            var tasks = new Task[10];

            var mutex = new Mutex();

            for (int i = 0; i < 10; i++)
            {
                tasks[i] = new Task(() =>
                {

                    //Thread.Sleep(10);
                    for (int j = 0; j < 1000; j++)
                    {
                        bool lockAcquired = mutex.WaitOne();
                        try
                        {
                            account.Balance = account.Balance + 1;
                        }
                        finally
                        {
                            if (lockAcquired)
                                mutex.ReleaseMutex();
                        }

                    }

                });
                tasks[i].Start();
            }

            Task.WaitAll(tasks);
            Console.WriteLine("Expected value {0}, Counter value: {1}", 10000, account.Balance);

            Console.WriteLine("finish");
            Console.ReadLine();
        }

        static void RWSlimTest()
        {
            var rwlock = new ReaderWriterLockSlim();

            CancellationTokenSource tSource = new CancellationTokenSource();

            Task[] tasks = new Task[5];

            for (int i = 0; i < 5; i++)
            {
                tasks[i] = new Task(() =>
                  {
                      while (true)
                      {
                          rwlock.EnterReadLock();
                          Console.WriteLine("Read lock acquired - count: {0}", rwlock.CurrentReadCount);
                          tSource.Token.WaitHandle.WaitOne(1000);

                          rwlock.ExitReadLock();
                          Console.WriteLine("Read lock release - count: {0}", rwlock.CurrentReadCount);

                          tSource.Token.ThrowIfCancellationRequested();
                      }
                  }, tSource.Token);

                tasks[i].Start();
            }

            // prompt the user
            Console.WriteLine("Press enter to acquire write lock");
            // wait for the user to press enter
            Console.ReadLine();
            // acquire the write lock
            Console.WriteLine("Requesting write lock");
            rwlock.EnterWriteLock();
            Console.WriteLine("Write lock acquired");
            Console.WriteLine("Press enter to release write lock");
            // wait for the user to press enter
            Console.ReadLine();
            // release the write lock
            rwlock.ExitWriteLock();
            // wait for 2 seconds and then cancel the tasks
            tSource.Token.WaitHandle.WaitOne(2000);
            tSource.Cancel();

            try
            {
                // wait for the tasks to complete
                Task.WaitAll(tasks);
            }
            catch (AggregateException)
            {
                // do nothing
            }
            // wait for input before exiting
            Console.WriteLine("Press enter to finish");
            Console.ReadLine();

        }

        static void QueueTest()
        {
            var sharedQueue = new ConcurrentQueue<int>();

            for (int i = 0; i < 1000; i++)
            {
                sharedQueue.Enqueue(i);
            }

            int itemCount = 0;

            Task[] tasks = new Task[10];

            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = new Task(() =>
                  {
                      while (sharedQueue.Count > 0)
                      {
                          int queueElement;
                          bool gotElement = sharedQueue.TryDequeue(out queueElement);
                          if(gotElement)
                            Interlocked.Increment(ref itemCount);
                      }
                  });
                tasks[i].Start();
            }

            Task.WaitAll(tasks);

            // report on the number of items processed
            Console.WriteLine("Items processed: {0}", itemCount);
            // wait for input before exiting
            Console.WriteLine("Press enter to finish");
            Console.ReadLine();
        }

        static void DictionaryTest()
        {
            var account = new BankAcount();

            var sharedDict = new ConcurrentDictionary<object, int>();

            var tasks = new Task<int>[10];

            for (int i = 0; i < tasks.Length; i++)
            {
                sharedDict.TryAdd(i, account.Balance);
                tasks[i] = new Task<int>((keyObj) =>
                  {
                      int currentValue;
                      bool gotValue;

                      for (int j = 0; j < 1000; j++)
                      {
                          gotValue = sharedDict.TryGetValue(keyObj, out currentValue);
                          sharedDict.TryUpdate(keyObj, currentValue + 1, currentValue);
                      }

                      int result;
                      gotValue = sharedDict.TryGetValue(keyObj, out result);
                      if (gotValue)
                      {
                          return result;
                      }else
                      {
                          throw new Exception(String.Format("No data item available for key {0}", keyObj));
                      }

                  },i);

                tasks[i].Start();
            }

            for (int i = 0; i < tasks.Length; i++)
            {
                account.Balance += tasks[i].Result;
            }

            // write out the counter value
            Console.WriteLine("Expected value {0}, Balance: {1}",
            10000, account.Balance);
            // wait for input before exiting
            Console.WriteLine("Press enter to finish");
            Console.ReadLine();
        }
    }

    class BankAcount
    {
        public int Balance
        {
            get;set;
        }
    }
}
