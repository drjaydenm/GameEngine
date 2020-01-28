namespace GameEngine.Core.Content
{
    public struct ContentManifestEntry
    {
        public string Key;
        public string Path;

        public override string ToString()
        {
            return Key + " | " + Path;
        }
    }
}
