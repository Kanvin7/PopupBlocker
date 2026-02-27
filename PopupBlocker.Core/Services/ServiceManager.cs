namespace PopupBlocker.Core.Services
{
    public class ServiceManager : IDisposable
    {
        private readonly Dictionary<int, IService> _services;

        public ServiceManager()
        {
            _services = [];
            _services.Add(ServiceType.DefaultLoggerService, new LoggerService());
            _services.Add(ServiceType.DefaultRuleConfigService, new RuleConfigService((LoggerService)_services[ServiceType.DefaultLoggerService]));
            _services.Add(ServiceType.DefaultPopupBlockService, new PopupBlockService((LoggerService)_services[ServiceType.DefaultLoggerService], (RuleConfigService)_services[ServiceType.DefaultRuleConfigService]));
            _services.Add(ServiceType.DefaultAutoRunService, new AutoRunService());
        }

        public T? GetService<T>(int sKey) where T : IService
        {
            if (_services.TryGetValue(sKey, out var service))
                return (T)service;
            else
                return default;
        }

        public bool RegisterService(int sKey, IService service) => _services.TryAdd(sKey, service);
        public bool UnregisterService(int sKey) => _services.Remove(sKey);

        #region IDisposable
        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)
                    foreach (var service in _services.Values)
                    {
                        var serviceType = service.GetType();
                        if (serviceType.GetInterface("IDisposable") is not null)
                            ((IDisposable)Convert.ChangeType(service, serviceType)).Dispose();
                    }
                }

                // TODO: 释放未托管的资源(未托管的对象)并重写终结器
                // TODO: 将大型字段设置为 null
                disposedValue = true;
            }
        }

        // // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
        // ~ServiceManager()
        // {
        //     // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
