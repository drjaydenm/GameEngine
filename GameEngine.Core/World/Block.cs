namespace GameEngine.Core.World
{
    public class Block
    {
        public bool IsActive { get; private set; }
        public uint BlockType { get; private set; }
        public object Data { get; set; }

        public Block(bool isActive, uint blockType, object data = null)
        {
            IsActive = isActive;
            BlockType = blockType;
            Data = data;
        }

        public void SetActive(bool isActive)
        {
            IsActive = isActive;
        }
    }
}
