using System.Collections.Generic;
using PartCover.Browser.Api;

namespace PartCover.Browser
{
    internal class ApplicationHost : IServiceContainer
    {
        private readonly List<object> services = new List<object>();

        public T GetService<T>() where T : class
        {
            lock (services)
            {
                foreach (object o in services)
                {
                    if (typeof(T).IsInstanceOfType(o))
                        return (T)o;
                }
            }
            return null;
        }

        public bool RegisterService<T>(T service) where T : class
        {
            lock (services)
            {
                if (services.Contains(service))
                    return false;

                services.Add(service);
                return true;
            }
        }

        public bool UnregisterService<T>(T service) where T : class
        {
            lock (services)
            {
                return 0 < services.RemoveAll(delegate(object actual)
                {
                    return (service == actual);
                });
            }
        }

        public void Build()
        {
            lock (services)
            {
                foreach (object o in services.ToArray())
                {
                    if (o is IFeature) ((IFeature)o).Attach(this);
                }

                foreach (object o in services.ToArray())
                {
                    if (o is IFeature) ((IFeature)o).Build(this);
                }
            }
        }

        public void Destroy()
        {
            lock (services)
            {
                foreach (object o in services.ToArray())
                {
                    if (o is IFeature) ((IFeature)o).Destroy(this);
                }

                foreach (object o in services.ToArray())
                {
                    if (o is IFeature) ((IFeature)o).Detach(this);
                }
                services.Clear();
            }
        }
    }
}
