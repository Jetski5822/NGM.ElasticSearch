using Elasticsearch.Net.Connection;
using Nest;
using Orchard;

namespace NGM.ElasticSearch.Settings {
    public interface IElasticSearchConnectionSettings : IDependency {
        IConnectionSettingsValues ConnectionSettings { get; }
        IConnection Connection { get; }
    }

    public class DefaultElasticSearchConnectionSettings : IElasticSearchConnectionSettings {
        public IConnectionSettingsValues ConnectionSettings {
            get { return new ConnectionSettings(); }
        }

        public IConnection Connection {
            get { return new HttpConnection(ConnectionSettings); }
        }
    }
}