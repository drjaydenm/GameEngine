using GameEngine.Core.Content.Raw;

namespace GameEngine.Core.Content.Importers
{
    public interface IContentImporter
    {
        string[] FileExtensions { get; }

        IContentRaw Import(string filePath);
    }

    public interface IContentImporter<TRaw> : IContentImporter where TRaw : IContentRaw
    {
        new TRaw Import(string filePath);
    }
}
