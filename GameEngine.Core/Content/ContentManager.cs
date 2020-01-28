using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace GameEngine.Core.Content
{
    public class ContentManager : IContentManager
    {
        private readonly string contentDirectory;
        private readonly Dictionary<string, IContentManifest> manifests;

        public ContentManager(string contentDirectory)
        {
            this.contentDirectory = contentDirectory;

            manifests = new Dictionary<string, IContentManifest>();
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

        public IContentManifest this[string manifestKey]
        {
            get { return manifests[manifestKey]; }
        }
    }
}
