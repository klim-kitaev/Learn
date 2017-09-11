using System;
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
            RWSlimTest();
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
    }

    class BankAcount
    {
        public int Balance
        {
            get;set;
        }
    }
}
