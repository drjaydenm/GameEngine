namespace GameEngine.Core.Content
{
    public interface IContentManifest
    {
        string this[string contentKey] { get; }
    }
}
