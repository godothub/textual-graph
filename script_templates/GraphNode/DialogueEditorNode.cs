// meta-name: 对话编辑器自定义节点模板
// meta-default: true
// meta-space-indent: 4

#if TOOLS
using System.Collections.Generic;
using _BINDINGS_NAMESPACE_;
using TextualGraph.Editor.EditorNode;

[Tool]
public partial class _CLASS_ : GraphNode, IGraphNode, IGraphNodeFactory<_CLASS_>
{
    /// <inheritdoc />
    public event MetadataChangedEventHandler MetadataChanged;

    /// <inheritdoc />
    public static string NodeName => throw new System.NotImplementedException();

    /// <inheritdoc />
    public static string PrefabFilePath => throw new System.NotImplementedException();

    /// <inheritdoc />
    public string NodeType => NodeName;

    /// <inheritdoc />
    public Dictionary<string, object> CustomData { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    /// <inheritdoc />
    public bool CanConnectWhenIsInput(GraphNode output, long outputPort, out long inputPort)
    {
        throw new System.NotImplementedException();
    }

    /// <inheritdoc />
    public bool CanConnectWhenIsOutput(GraphNode input, long inputPort, out long outputPort)
    {
        throw new System.NotImplementedException();
    }

    /// <inheritdoc />
    public int GetMaxInputConnections(long inputPort)
    {
        throw new System.NotImplementedException();
    }

    /// <inheritdoc />
    public int GetMaxOutputConnections(long outputPort)
    {
        throw new System.NotImplementedException();
    }

    /// <inheritdoc />
    public HashSet<string> GetValidFromNodeNamesForPort(long inputPort)
    {
        throw new System.NotImplementedException();
    }

    /// <inheritdoc />
    public HashSet<string> GetValidToNodeNamesForPort(long outputPort)
    {
        throw new System.NotImplementedException();
    }
}
#endif