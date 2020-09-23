#region Using Statements
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
#endregion

namespace Samples.CmdMultithreading
{
    public class SecondaryAppThread : ISecondaryAppThread
    {
        ILogger<SecondaryAppThread> _logger;
        IConfiguration _configuration;

        public int Id { get; set; }

        public event SecondaryAppThreadTerminatingEventHandler Terminating;
        public string ErrorMessage { get; set; }
        public bool HasError
        {
            get { return !string.IsNullOrEmpty(this.ErrorMessage); }
        }

        public SecondaryAppThread(ILogger<SecondaryAppThread> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public void Execute()
        {
            try
            {
                //do work here (call a service calls)
                int wait = new Random().Next(100, 1000);
                _logger.LogInformation("Thread " + this.Id + " waiting for " + wait.ToString());
                System.Threading.Thread.Sleep(wait);

            }
            finally
            {
                this.Terminating(this.Id);
            }
            return;
        }
    }
}
