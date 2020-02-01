﻿using System;
using System.Collections.Generic;
using System.IO;
using GameEngine.Core.Content.Importers;
using GameEngine.Core.Content.Processors;
using GameEngine.Core.Content.Raw;
using Newtonsoft.Json;

namespace GameEngine.Core.Content
{
    public class ContentManager : IContentManager
    {
        public IReadOnlyCollection<IContentImporter> Importers => importers;
        public IReadOnlyDictionary<string, IContentManifest> Manifests => manifests;
        public IReadOnlyCollection<IContentProcessor> Processors => processors;

        private readonly Engine engine;
        private readonly string contentDirectory;
        private readonly List<IContentImporter> importers;
        private readonly Dictionary<string, IContentManifest> manifests;
        private readonly List<IContentProcessor> processors;

        public ContentManager(Engine engine, string contentDirectory)
        {
            this.engine = engine;
            this.contentDirectory = contentDirectory;

            importers = new List<IContentImporter>();
            manifests = new Dictionary<string, IContentManifest>();
            processors = new List<IContentProcessor>();
        }

        public void AddDefaultImportersAndProcessors()
        {
            AddImporter(new Texture2DImporter());

            AddProcessor(new Texture2DProcessor(engine));
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
                using (var sr = new StreamReader(Path.Combine(contentDirectory, manifestFiles[i] + ".manifest.json")))
                {
                    var manifestFile = sr.ReadToEnd();

                    var manifest = JsonConvert.DeserializeObject<ContentManifest>(manifestFile);
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

            var extension = Path.GetExtension(contentPath);
            var importer = GetImporter(extension);
            if (importer == null)
                throw new Exception("Cannot find an importer for this extension");

            IContentRaw contentRaw;
            using (var fs = new FileStream(contentPath, FileMode.Open, FileAccess.Read))
            {
                contentRaw = importer.Import(fs);
            }

            var processor = GetProcessor(contentRaw.GetType());
            if (processor == null)
                throw new Exception("Cannot find a processor for this extension");

            return (T)processor.Process(contentRaw);
        }

        private IContentImporter GetImporter(string extension)
        {
            for (var i = 0; i < importers.Count; i++)
            {
                for (var j = 0; j < importers[i].FileExtensions.Length; j++)
                {
                    if (importers[i].FileExtensions[j] == extension)
                        return importers[i];
                }
            }

            return null;
        }

        private IContentProcessor GetProcessor(Type rawType)
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

                    if (interfaces[j].GenericTypeArguments[0] == rawType)
                        return processors[i];
                }
            }

            return null;
        }
    }
}
