using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Maestro.Web.Data
{
    public class TimedService : IHostedService, IDisposable
    {
        private Timer _timer = null;

        public TimedService()
        {

        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _timer = new Timer(DoWork, null, TimeSpan.Zero,
                TimeSpan.FromSeconds(5));

            return Task.CompletedTask;
        }

        private void DoWork(object state)
        {
            foreach (var aircraft in Functions.AircraftData.ToList())
            {
                if (DateTime.UtcNow.Subtract(aircraft.UpdateUTC) > TimeSpan.FromMinutes(1))
                {
                    Functions.Remove(aircraft);
                    continue;
                }
            }
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
