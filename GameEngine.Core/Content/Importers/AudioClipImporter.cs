using System;
using GameEngine.Core.Audio;
using GameEngine.Core.Content.Raw;

namespace GameEngine.Core.Content.Importers
{
    public class AudioClipImporter : IContentImporter<AudioClipRaw>
    {
        public string[] FileExtensions => new[] { ".wav" };

        public AudioClipRaw Import(IContentLoader loader, string filePath)
        {
            using var s = loader.OpenStream(filePath);
            var waveFile = new WaveFile(s);

            return new AudioClipRaw(waveFile);
        }

        IContentRaw IContentImporter.Import(IContentLoader loader, string filePath)
        {
            return Import(loader, filePath);
        }
    }
}
