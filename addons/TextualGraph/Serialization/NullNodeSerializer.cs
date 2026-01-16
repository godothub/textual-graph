#if TOOLS

using System;

namespace TextualGraph.Serialization;

/// <summary>
/// 空节点序列化器，用于处理不支持序列化的节点类型
/// </summary>
public sealed class NullNodeSerializer : NodeSerializer
{
    private readonly string _nodeType;

    public NullNodeSerializer(string nodeType)
    {
        _nodeType = nodeType;
    }

    public override string NodeType => _nodeType;

    public override bool CanSerialize(NodeData node, IGraphSerializationContext context)
        => false;

    public override bool CanDeserialize(ParsedNodeFragment fragment)
        => false;

    public override string Serialize(NodeData node, IGraphSerializationContext context)
        => throw new NotSupportedException();

    public override NodeDeserializeResult Deserialize(string text)
        => throw new NotSupportedException();
}

#endif