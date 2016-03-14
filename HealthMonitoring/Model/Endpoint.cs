using System;
using System.Threading;
using System.Threading.Tasks;
using HealthMonitoring.Monitors;

namespace HealthMonitoring.Model
{
    public class Endpoint : IDisposable
    {
        private readonly IHealthMonitor _monitor;

        public Endpoint(Guid id, IHealthMonitor monitor, string address, string name, string group)
        {
            Id = id;
            _monitor = monitor;
            Address = address;
            Name = name;
            Group = group;
            UpdateLastModifiedTime();
        }

        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public string Address { get; }
        public string MonitorType => _monitor.Name;
        public IHealthMonitor Monitor => _monitor;
        public string Group { get; private set; }
        public bool IsDisposed { get; private set; }
        public EndpointHealth Health { get; private set; }
        public DateTimeOffset LastModifiedTime { get; private set; }
        public Endpoint Update(string group, string name)
        {
            Group = group;
            Name = name;
            UpdateLastModifiedTime();
            return this;
        }

        public async Task CheckHealth(IHealthSampler sampler, CancellationToken cancellationToken)
        {
            var health = await sampler.CheckHealth(this, cancellationToken);
            if (!IsDisposed)
            {
                Health = health;
                UpdateLastModifiedTime();
            }
        }

        public void Dispose()
        {
            IsDisposed = true;
            Health = null;
            UpdateLastModifiedTime();
        }

        private void UpdateLastModifiedTime()
        {
            LastModifiedTime = DateTimeOffset.UtcNow;
        }

        public override string ToString()
        {
            return $"{Group}/{Name} ({MonitorType}: {Address})";
        }
    }
}