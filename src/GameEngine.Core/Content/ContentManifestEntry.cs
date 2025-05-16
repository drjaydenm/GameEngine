namespace GameEngine.Core.Content
{
    public struct ContentManifestEntry
    {
        public string Key { get; set; }
        public string Path { get; set; }

        public override string ToString()
        {
            return Key + " | " + Path;
        }
    }
}
