namespace GameEngine.Core.World
{
    public struct Block
    {
        public bool IsActive;
        public byte BlockType;

        public Block(bool isActive, byte blockType)
        {
            IsActive = isActive;
            BlockType = blockType;
        }
    }
}
