using System.Collections.Generic;

namespace GameEngine.Core.Content
{
    public interface IContentManifest
    {
        IReadOnlyCollection<ContentManifestEntry> Contents { get; }

        string this[string contentKey] { get; }
    }
}
