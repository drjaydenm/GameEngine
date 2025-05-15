namespace GameEngine.Core.Content;

public class ContentManifest : IContentManifest
{
    public IReadOnlyCollection<ContentManifestEntry> Contents { get; set; }

    public string this[string contentKey]
    {
        get
        {
            foreach (var entry in Contents)
            {
                if (entry.Key == contentKey)
                {
                    return entry.Path;
                }
            }

            throw new KeyNotFoundException("Cannot find content key " + contentKey);
        }
    }
}
