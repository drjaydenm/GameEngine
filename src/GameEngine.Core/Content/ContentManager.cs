using System.Text.Json;
using GameEngine.Core.Content.Importers;
using GameEngine.Core.Content.Processors;

namespace GameEngine.Core.Content
{
    public class ContentManager : IContentManager
    {
        public IReadOnlyCollection<IContentImporter> Importers => importers;
        public IReadOnlyDictionary<string, IContentManifest> Manifests => manifests;
        public IReadOnlyCollection<IContentProcessor> Processors => processors;

        private readonly Engine engine;
        private readonly string contentDirectory;
        private readonly IContentLoader loader;
        private readonly List<IContentImporter> importers;
        private readonly Dictionary<string, IContentManifest> manifests;
        private readonly List<IContentProcessor> processors;

        public ContentManager(Engine engine, string contentDirectory, IContentLoader loader)
        {
            this.engine = engine;
            this.contentDirectory = contentDirectory;
            this.loader = loader;

            importers = new List<IContentImporter>();
            manifests = new Dictionary<string, IContentManifest>();
            processors = new List<IContentProcessor>();
        }

        public void AddDefaultImportersAndProcessors()
        {
            AddImporter(new AudioClipImporter());
            AddImporter(new MeshImporter());
            AddImporter(new ShaderImporter());
            AddImporter(new Texture2DImporter());
            AddImporter(new TextureArrayImporter());

            AddProcessor(new AudioClipProcessor(engine));
            AddProcessor(new MeshProcessor());
            AddProcessor(new ShaderProcessor(engine));
            AddProcessor(new Texture2DProcessor(engine));
            AddProcessor(new TextureArrayProcessor(engine));
        }

        public void AddImporter(IContentImporter importer)
        {
            if (!importers.Contains(importer))
                importers.Add(importer);
        }

        public void AddProcessor(IContentProcessor processor)
        {
            if (!processors.Contains(processor))
                processors.Add(processor);
        }

        public void LoadManifests(params string[] manifestFiles)
        {
            for (var i = 0; i < manifestFiles.Length; i++)
            {
                var manifestPath = Path.Combine(contentDirectory, manifestFiles[i] + ".manifest.json");
                using (var sr = new StreamReader(loader.OpenStream(manifestPath)))
                {
                    var manifestFile = sr.ReadToEnd();

                    var manifest = JsonSerializer.Deserialize<ContentManifest>(manifestFile);
                    var manifestKey = Path.GetFileName(manifestFiles[i]);

                    manifests.Add(manifestKey, manifest);
                }
            }
        }

        public T Load<T>(string manifestKey, string contentKey) where T : IContent
        {
            if (!manifests.TryGetValue(manifestKey, out var manifest))
                throw new Exception("Cannot find manifest by key");

            var contentPath = Path.Combine(contentDirectory, manifest[contentKey]);
            if (string.IsNullOrEmpty(contentPath))
                throw new Exception("Cannot find content by key");

            var importer = GetImporter(manifest[contentKey]);
            if (importer == null)
                throw new Exception("Cannot find an importer for this extension");

            var contentRaw = importer.Import(loader, contentPath);

            var rawType = contentRaw.GetType();
            var processor = GetProcessor(rawType, typeof(T));
            if (processor == null)
                throw new Exception("Cannot find a processor for " + rawType.Name + " -> " + typeof(T).Name);

            return (T)processor.Process(contentRaw);
        }

        private IContentImporter GetImporter(string filePath)
        {
            for (var i = 0; i < importers.Count; i++)
            {
                for (var j = 0; j < importers[i].FileExtensions.Length; j++)
                {
                    if (filePath.EndsWith(importers[i].FileExtensions[j]))
                        return importers[i];
                }
            }

            return null;
        }

        private IContentProcessor GetProcessor(Type rawType, Type outType)
        {
            for (var i = 0; i < processors.Count; i++)
            {
                var processorType = processors[i].GetType();
                var interfaces = processorType.GetInterfaces();

                for (var j = 0; j < interfaces.Length; j++)
                {
                    if (!interfaces[j].IsGenericType)
                        continue;

                    if (interfaces[j].GetGenericTypeDefinition() != typeof(IContentProcessor<,>))
                        continue;

                    if (interfaces[j].GenericTypeArguments[0] == rawType
                        && interfaces[j].GenericTypeArguments[1] == outType)
                        return processors[i];
                }
            }

            return null;
        }
    }
}
