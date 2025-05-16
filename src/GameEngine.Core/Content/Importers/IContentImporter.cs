using GameEngine.Core.Content.Raw;

namespace GameEngine.Core.Content.Importers
{
    public interface IContentImporter
    {
        string[] FileExtensions { get; }

        IContentRaw Import(IContentLoader loader, string filePath);
    }

    public interface IContentImporter<TRaw> : IContentImporter where TRaw : IContentRaw
    {
        new TRaw Import(IContentLoader loader, string filePath);
    }
}
