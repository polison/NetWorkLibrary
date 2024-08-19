using NetWorkLibrary.Utility;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NetWorkLibrary.Sample
{
    public class FileLog : BackgroundService, ILog
    {
        private readonly FileStream fs;
        private readonly TextWriter writer;

        private DateTime StartTime;

        public FileLog(string path)
        {
            fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write);
            writer = new StreamWriter(fs);
            StartTime = DateTime.Now.AddDays(-1);

            StartAsync();
        }

        ~FileLog()
        {
            writer.Flush();
            writer.Close();
            fs.Close();

            StopAsync().Wait();
        }

        public void Error(string format, params object[] args)
        {
            writer.WriteLine($"[Error]{format}", args);
            writer.Flush();
        }

        public void Message(string format, params object[] args)
        {
            writer.WriteLine($"[Message]{format}", args);
            writer.Flush();
        }

        public void Warning(string format, params object[] args)
        {
            writer.WriteLine($"[Warning]{format}", format, args);
            writer.Flush();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                DateTime dateTime = DateTime.Now;
                if (dateTime.Day != StartTime.Day)
                {
                    StartTime = dateTime;
                    fs.Position = 0;
                    fs.SetLength(0);
                    writer.WriteLine("New Day Logged!");
                    writer.Flush();
                }
                await Task.Delay(1000);
            }
        }
    }
}
