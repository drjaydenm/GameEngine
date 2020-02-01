using System.Collections.Generic;
using GameEngine.Core.Content.Importers;
using GameEngine.Core.Content.Processors;

namespace GameEngine.Core.Content
{
    public interface IContentManager
    {
        IReadOnlyCollection<IContentImporter> Importers { get; }
        IReadOnlyDictionary<string, IContentManifest> Manifests { get; }
        IReadOnlyCollection<IContentProcessor> Processors { get; }

        void AddDefaultImportersAndProcessors();
        void AddImporter(IContentImporter importer);
        void AddProcessor(IContentProcessor processor);
        void LoadManifests(params string[] manifestFiles);

        T Load<T>(string manifestKey, string contentKey) where T : IContent;
    }
}
