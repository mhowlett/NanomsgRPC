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
        private readonly string _maxWaitForAvailableConnectionMillisecondsName;

        public NanoConnectionPool_FromConfig(
            string connectionTypeName,
            string portConfigurationName,
            string hostConfigurationName,
            string connectionPoolSizeConfigurationName,
            string connectionPoolTimeoutConfigurationName,
            string maxWaitForAvailableConnectionMillisecondsName)
        {
            _connectionTypeName = connectionTypeName;
            _portConfigurationName = portConfigurationName;
            _hostConfigurationName = hostConfigurationName;
            _connectionPoolSizeConfigurationName = connectionPoolSizeConfigurationName;
            _connectionPoolTimeoutConfigurationName = connectionPoolTimeoutConfigurationName;
            _maxWaitForAvailableConnectionMillisecondsName = maxWaitForAvailableConnectionMillisecondsName;
        }

        public override string Host
        {
            get
            {
                string result = System.Web.Configuration.WebConfigurationManager.AppSettings[_hostConfigurationName] 
                    ?? ConfigurationSettings.AppSettings[_hostConfigurationName];

                return result;
            }
        }

        public override int Port
        {
            get
            {
                string result = System.Web.Configuration.WebConfigurationManager.AppSettings[_portConfigurationName] 
                    ?? ConfigurationSettings.AppSettings[_portConfigurationName];

                return Convert.ToInt32(result);
            }
        }

        public override int ConnectionPoolSize
        {
            get
            {
                if (_connectionPoolSizeConfigurationName == null)
                {
                    return 1;
                }

                string result = System.Web.Configuration.WebConfigurationManager.AppSettings[_connectionPoolSizeConfigurationName]
                  ?? ConfigurationSettings.AppSettings[_connectionPoolSizeConfigurationName];

                if (result != null)
                {
                    return Convert.ToInt32(result);
                }

                return 1;
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
                if (_connectionPoolTimeoutConfigurationName == null)
                {
                    return TimeSpan.FromSeconds(30);
                }

                string result = System.Web.Configuration.WebConfigurationManager.AppSettings[_connectionPoolTimeoutConfigurationName]
                    ?? ConfigurationSettings.AppSettings[_connectionPoolTimeoutConfigurationName];

                if (result == null)
                {
                    return TimeSpan.FromSeconds(30);
                }

                return TimeSpan.FromSeconds(int.Parse(result));
            }
        }

        public override TimeSpan MaxWaitForAvailableConnection
        {
            get
            {
                if (_maxWaitForAvailableConnectionMillisecondsName == null)
                {
                    return TimeSpan.FromSeconds(1);
                }

                string result = System.Web.Configuration.WebConfigurationManager.AppSettings[_maxWaitForAvailableConnectionMillisecondsName]
                    ?? ConfigurationSettings.AppSettings[_maxWaitForAvailableConnectionMillisecondsName];

                if (result == null)
                {
                    return TimeSpan.FromSeconds(1);
                }

                return TimeSpan.FromMilliseconds(int.Parse(result));                
            }
        }
    }
}
