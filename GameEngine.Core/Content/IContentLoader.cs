using System.IO;

namespace GameEngine.Core.Content
{
    public interface IContentLoader
    {
        Stream OpenStream(string contentPath);
    }
}
