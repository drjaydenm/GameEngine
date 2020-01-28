using System.Collections.Generic;

namespace GameEngine.Core.Content
{
    public class ContentManifest : IContentManifest
    {
        public IReadOnlyCollection<ContentManifestEntry> Contents => contents;

        public string this[string contentKey]
        {
            get
            {
                for (var i = 0; i < contents.Count; i++)
                {
                    if (contents[i].Key == contentKey)
                        return contents[i].Path;
                }
                return null;
            }
        }

        private List<ContentManifestEntry> contents;

        public ContentManifest()
        {
            contents = new List<ContentManifestEntry>();
        }
    }
}
