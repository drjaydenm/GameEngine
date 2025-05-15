namespace GameEngine.Core.Content.Raw
{
    public class MeshRaw : IContentRaw
    {
        public List<MeshPieceRaw> Pieces { get; }

        public MeshRaw()
        {
            Pieces = new List<MeshPieceRaw>();
        }
    }
}
