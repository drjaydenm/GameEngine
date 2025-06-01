namespace GameEngine.Core.Graphics;

public readonly struct GraphicsPipelineDescription : IEquatable<GraphicsPipelineDescription>
{
    public BlendStateDescription BlendState { get; init; }
    public DepthStencilStateDescription DepthStencilState { get; init; }
    public RasterizerStateDescription RasterizerState { get; init; }
    public PrimitiveType PrimitiveType { get; init; }
    public ShaderSetDescription ShaderSet { get; init; }
    public IResourceLayout[] ResourceLayouts { get; init; }
    public OutputDescription Output { get; init; }

    public GraphicsPipelineDescription(BlendStateDescription blendState, DepthStencilStateDescription depthStencilState,
        RasterizerStateDescription rasterizerState, PrimitiveType primitiveType, ShaderSetDescription shaderSet,
        IResourceLayout[] resourceLayouts, OutputDescription output)
    {
        BlendState = blendState;
        DepthStencilState = depthStencilState;
        RasterizerState = rasterizerState;
        PrimitiveType = primitiveType;
        ShaderSet = shaderSet;
        ResourceLayouts = resourceLayouts;
        Output = output;
    }

    public bool Equals(GraphicsPipelineDescription other)
    {
        return BlendState.Equals(other.BlendState)
            && DepthStencilState.Equals(other.DepthStencilState)
            && RasterizerState.Equals(other.RasterizerState)
            && PrimitiveType == other.PrimitiveType
            && ShaderSet.Equals(other.ShaderSet)
            && Equals(ResourceLayouts, other.ResourceLayouts)
            && Output.Equals(other.Output);
    }

    public override bool Equals(object obj)
    {
        return obj is GraphicsPipelineDescription other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(BlendState, DepthStencilState, RasterizerState,
            (int)PrimitiveType, ShaderSet, ResourceLayouts, Output);
    }

    public static bool operator ==(GraphicsPipelineDescription left, GraphicsPipelineDescription right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(GraphicsPipelineDescription left, GraphicsPipelineDescription right)
    {
        return !(left == right);
    }
}
