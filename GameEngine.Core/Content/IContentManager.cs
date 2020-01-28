namespace GameEngine.Core.Content
{
    public interface IContentManager
    {
        IContentManifest this[string manifestKey] { get; }

        void LoadManifests(params string[] manifestFiles);
    }
}
