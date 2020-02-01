using System.IO;
using GameEngine.Core.Content.Raw;

namespace GameEngine.Core.Content.Importers
{
    public interface IContentImporter
    {
        string[] FileExtensions { get; }

        IContentRaw Import(Stream stream);
    }

    public interface IContentImporter<TRaw> : IContentImporter where TRaw : IContentRaw
    {
        new TRaw Import(Stream stream);
    }
}
