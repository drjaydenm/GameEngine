namespace GameEngine.Core.Content
{
    public class FileContentLoader : IContentLoader
    {
        public Stream OpenStream(string contentPath)
        {
            return new FileStream(contentPath, FileMode.Open, FileAccess.Read);
        }
    }
}
