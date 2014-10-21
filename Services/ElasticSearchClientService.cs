using Nest;
using NGM.ElasticSearch.Settings;
using Orchard;

namespace NGM.ElasticSearch.Services {
    public interface IElasticSearchClientService : IDependency {
        IElasticClient Client();
    }

    public class ElasticSearchClientService : IElasticSearchClientService {
        private readonly IElasticSearchConnectionSettings _elasticSearchConnectionSettings;

        public ElasticSearchClientService(IElasticSearchConnectionSettings elasticSearchConnectionSettings) {
            _elasticSearchConnectionSettings = elasticSearchConnectionSettings;
        }

        public IElasticClient Client() {
            return new ElasticClient(
                _elasticSearchConnectionSettings.ConnectionSettings,
                _elasticSearchConnectionSettings.Connection,
                new NestSerializer(_elasticSearchConnectionSettings.ConnectionSettings));
        }
    }

    
}