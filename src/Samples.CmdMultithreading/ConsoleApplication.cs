#region Using Statements
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Samples.CmdMultithreading
{
    public class ConsoleApplication
    {
        ILogger<ConsoleApplication> _logger;
        IConfiguration _configuration;
        private static ConcurrentDictionary<int, object> _runningThreads;
        ISecondaryAppThread _secondaryAppThread;

        public string ErrorMessage { get; set; }

        public bool HasError
        {
            get { return !string.IsNullOrEmpty(this.ErrorMessage); }
        }

        public ConsoleApplication(ILogger<ConsoleApplication> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _runningThreads = new ConcurrentDictionary<int, object>();
        }

        public async Task Run()
        {
            int maxThreadCount = _configuration.GetValue<int>("maxThreadCount");
            _logger.LogInformation("Starting Run with " + maxThreadCount + " threads");

            //create number of threads based on a config value
            for (int id = 1; id <= maxThreadCount; id++)
            {
                if (!_runningThreads.ContainsKey(id))
                {
                    _secondaryAppThread = Program.ServiceScope.ServiceProvider.GetRequiredService<ISecondaryAppThread>();
                    _secondaryAppThread.Id = id;
                    _secondaryAppThread.Terminating += new SecondaryAppThreadTerminatingEventHandler(this.TerminateEventHandler);

                    //Create and start thread
                    Thread oThread = new Thread(new ThreadStart(_secondaryAppThread.Execute));
                    oThread.Name = id.ToString();
                    oThread.SetApartmentState(ApartmentState.MTA);
                    if (_runningThreads.TryAdd(id, oThread))
                    {
                        oThread.Start();
                        _logger.LogInformation("Thread " + id + " starting.");
                    }
                }
            }

            //wait until all threads have terminated
            while (_runningThreads.Count > 0)
            {
                System.Threading.Thread.Sleep(1000);
            };
        }

        private void TerminateEventHandler(int id)
        {
            try
            {
                if (_runningThreads != null && _runningThreads.ContainsKey(id))
                {
                    object output;
                    _runningThreads.TryRemove(id, out output);
                    _logger.LogInformation("Thread " + id + " terminating.");
                }
            }
            catch (Exception ex)
            {
                this.ErrorMessage = "Error terminating thread: " + ex.ToString();
                _logger.LogError(this.ErrorMessage);
            }
        }


    }
}
