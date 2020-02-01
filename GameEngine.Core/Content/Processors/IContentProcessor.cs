using GameEngine.Core.Content.Raw;

namespace GameEngine.Core.Content.Processors
{
    public interface IContentProcessor
    {
        IContent Process(IContentRaw contentRaw);
    }

    public interface IContentProcessor<TRaw, TOut> : IContentProcessor
        where TRaw : IContentRaw
        where TOut : IContent
    {
        TOut Process(TRaw contentRaw);
    }
}
