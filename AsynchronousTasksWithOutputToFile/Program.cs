using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AsynchronousTasksWithOutputToFile
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            
            AsyncFileAccess access = new AsyncFileAccess();

            Task task1 = new Task(() =>
            {
                Console.WriteLine("Starting task 1");
                Task.Delay(access.GetDelay()).Wait();
                access.FileUpdate("Task 1");
                Console.WriteLine("Task 1 complete");
            });
            


            Task task2 = new Task(() =>
            {
                Console.WriteLine("Starting task 2");
                Task.Delay(access.GetDelay()).Wait();
                access.FileUpdate("Task 2");
                Console.WriteLine("Task 2 complete");
            });
            

            Task task3 = new Task(() =>
            {
                Console.WriteLine("Starting task 3");
                Task.Delay(access.GetDelay()).Wait();
                access.FileUpdate("Task 3");
                Console.WriteLine("Task 3 complete");
            });

            List<Task> tasks = new List<Task>
            {
                task1,
                task2,
                task3
            };

            Parallel.For(0, tasks.Count(), index => tasks[index].Start());

            await Task.WhenAll(tasks);

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
    }

    public class AsyncFileAccess
    {
        private const string _filePath = @"C:\Users\david\AppData\local\async.txt";
        private static object fileLock = new object();

        public AsyncFileAccess()
        {
            if (!File.Exists(_filePath))
            {
                Console.WriteLine("Creating file....");
                CreateFile(_filePath);
                Console.WriteLine("File created....");
            }
        }

        private static void CreateFile(string destinationFilePath)
        {
            lock(fileLock)
            {
                
                FileStream destFile = new FileStream(destinationFilePath,
                                                    FileMode.Create,
                                                    FileAccess.Write);
                using (StreamWriter writer = new StreamWriter(destFile))
                {
                    writer.WriteLine("1");
                    writer.WriteLine("2");
                    writer.Write("3");
                    writer.Close();
                }

                destFile.Close();
            }

        }

        public int GetDelay()
        {
            Random random = new Random();
            return random.Next(1000, 5000);
        }

        public void FileUpdate(string task)
        {
            lock (fileLock)
            {
                Thread.Sleep(GetDelay());
                long lineCounter = 0L;
                var reader = new StreamReader(_filePath);

                using (reader)
                {
                    while (reader.ReadLine() != null)
                    {
                        lineCounter++;
                    }
                }
                Console.WriteLine("The number of lines: " + lineCounter.ToString());
                Console.WriteLine("Appending the line with: " + (lineCounter + 1).ToString() + " from: {0}", task);
                Task.Run(() =>
                {
                    File.AppendAllText(_filePath, Environment.NewLine + (++lineCounter).ToString());
                    reader.Close();
                });
            }

            
        }
    }
}
