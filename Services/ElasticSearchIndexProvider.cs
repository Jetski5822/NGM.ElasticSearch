using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Orchard.ContentManagement.MetaData;
using Orchard.Indexing;
using Nest;
using Orchard.Localization;
using Orchard.Logging;

namespace NGM.ElasticSearch.Services {
    public class ElasticSearchIndexProvider : IIndexProvider {
        private readonly IElasticSearchClientService _elasticSearchClientService;
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public ElasticSearchIndexProvider(IElasticSearchClientService elasticSearchClientService,
            IContentDefinitionManager contentDefinitionManager) {
            _elasticSearchClientService = elasticSearchClientService;
            _contentDefinitionManager = contentDefinitionManager;

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
        }

        public Localizer T { get; set; }
        public ILogger Logger { get; set; }

        private IElasticClient Client {
            get { return _elasticSearchClientService.Client(); }
        }

        public void CreateIndex(string name) {
            Client.CreateIndex(name);
        }

        public bool Exists(string name) {
            return Client.IndexExists(i => i.Index(name)).Exists;
        }

        public IEnumerable<string> List() {
            return Client.IndicesStats(i => i).Indices.Select(x => x.Key).Distinct();
        }

        public void DeleteIndex(string name) {
            Client.DeleteIndex(i => i.Index(name));
        }

        public bool IsEmpty(string indexName) {
            if (!Exists(indexName)) {
                return true;
            }

            return NumDocs(indexName) == 0;
        }

        public int NumDocs(string indexName) {
            return Convert.ToInt32(Client.IndicesStats(i => i.Index(indexName)).Stats.Total.Documents.Count);
        }

        public IDocumentIndex New(int documentId) {
            return new ElasticSearchDocumentIndex(documentId, T);
        }

        public void Store(string indexName, IDocumentIndex indexDocument) {
            Store(indexName, new[] { (ElasticSearchDocumentIndex)indexDocument });
        }

        public void Store(string indexName, IEnumerable<IDocumentIndex> indexDocuments) {
            Store(indexName, indexDocuments.Cast<ElasticSearchDocumentIndex>());
        }


        public void Store(string indexName, IEnumerable<ElasticSearchDocumentIndex> indexDocuments) {
            indexDocuments = indexDocuments.ToArray();

            if (!indexDocuments.Any()) {
                return;
            }

            Client.Bulk(bd =>
                bd.IndexMany(indexDocuments,
                (descriptor, s) => descriptor.Index(indexName)));
        }

        public void Delete(string indexName, int documentId) {
            Client.Delete(documentId.ToString(CultureInfo.InvariantCulture), i => i.Index(indexName));
        }

        public void Delete(string indexName, IEnumerable<int> documentIds) {
            Client.Bulk(bd => 
                bd.DeleteMany(documentIds.Select(x => x.ToString(CultureInfo.InvariantCulture)), 
                (descriptor, s) => descriptor.Index(indexName)));
        }

        public ISearchBuilder CreateSearchBuilder(string indexName) {
            return new ElasticSearchBuilder(Client, indexName);
        }

        public IEnumerable<string> GetFields(string indexName) {
            if (!Exists(indexName)) {
                return Enumerable.Empty<string>();
            }

            return Client.GetIndexSettings(i => i.Index(indexName)).IndexSettings.Mappings.Select(x => x.Name.Name);
        }
    }
}