using System;
using System.Configuration;

namespace NanomsgRPC.API
{
    public class NanoConnectionPool_FromConfig : NanoConnectionPool
    {
        private readonly string _connectionTypeName;
        private readonly string _portConfigurationName;
        private readonly string _hostConfigurationName;
        private readonly string _connectionPoolSizeConfigurationName;
        private readonly string _connectionPoolTimeoutConfigurationName;

        public NanoConnectionPool_FromConfig(
            string connectionTypeName,
            string portConfigurationName,
            string hostConfigurationName,
            string connectionPoolSizeConfigurationName,
            string connectionPoolTimeoutConfigurationName)
        {
            _connectionTypeName = connectionTypeName;
            _portConfigurationName = portConfigurationName;
            _hostConfigurationName = hostConfigurationName;
            _connectionPoolSizeConfigurationName = connectionPoolSizeConfigurationName;
            _connectionPoolTimeoutConfigurationName = connectionPoolTimeoutConfigurationName;
        }

        public override string Host
        {
            get
            {
                string result = System.Web.Configuration.WebConfigurationManager.AppSettings[_hostConfigurationName];

                if (result == null)
                {
                    result = ConfigurationSettings.AppSettings[_hostConfigurationName];
                }

                return result;
            }
        }

        public override int Port
        {
            get
            {
                string result = System.Web.Configuration.WebConfigurationManager.AppSettings[_portConfigurationName];

                if (result == null)
                {
                    result = ConfigurationSettings.AppSettings[_portConfigurationName];
                }

                return Convert.ToInt32(result);
            }
        }

        public override int ConnectionPoolSize
        {
            get
            {
                string result =
                  System.Web.Configuration.WebConfigurationManager.AppSettings[_connectionPoolSizeConfigurationName];

                if (result == null)
                {
                    result = ConfigurationSettings.AppSettings[_connectionPoolSizeConfigurationName];
                }

                if (result != null)
                {
                    return Convert.ToInt32(result);
                }

                return 0;
            }
        }

        public override string ConnectionTypeName
        {
            get { return _connectionTypeName; }
        }

        public override TimeSpan ConnectionTimeout
        {
            get
            {
                string result = System.Web.Configuration.WebConfigurationManager.AppSettings[_connectionPoolTimeoutConfigurationName];

                if (result == null)
                {
                    result = ConfigurationSettings.AppSettings[_connectionPoolTimeoutConfigurationName];
                }

                if (result == null)
                {
                    return TimeSpan.FromSeconds(30);
                }

                return TimeSpan.FromSeconds(int.Parse(result));
            }
        }
    }
}
