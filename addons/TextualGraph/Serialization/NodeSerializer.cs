#if TOOLS
using TextualGraph.Editor.EditorNode;

namespace TextualGraph.Serialization;


/// <summary>
/// 节点序列化器的抽象基类，用于定义如何序列化和反序列化对话编辑器中的节点
/// </summary>
public abstract class NodeSerializer
{
    /// <summary>
    /// 获取此序列化器支持的节点类型名称，对应<see cref="IGraphNodeFactory{T}.NodeName"/> 和 <see cref="IGraphNode.NodeType"/>
    /// </summary>
    public abstract string NodeType { get; }

    /// <summary>
    /// 判断当前序列化器是否能够序列化指定的节点
    /// </summary>
    /// <param name="node">要检查的节点数据</param>
    /// <param name="context">图序列化上下文，提供对图中其他节点的访问</param>
    /// <returns>如果可以序列化则返回true，否则返回false</returns>
    public virtual bool CanSerialize(NodeData node, IGraphSerializationContext context)
        => true;
    
    /// <summary>
    /// 判断当前序列化器是否能够从指定的节点片段中反序列化节点数据
    /// </summary>
    /// <param name="fragment">要检查的节点片段</param>
    /// <returns>如果可以反序列化则返回true，否则返回false</returns>
    public virtual bool CanDeserialize(ParsedNodeFragment fragment)
        => true;

    /// <summary>
    /// 将节点数据序列化为字符串表示形式
    /// </summary>
    /// <param name="node">要序列化的节点数据</param>
    /// <param name="context">图序列化上下文，提供对图中其他节点的访问</param>
    /// <returns>序列化后的节点文本表示</returns>
    public abstract string Serialize(
        NodeData node,
        IGraphSerializationContext context
    );

    /// <summary>
    /// 从字符串表示形式反序列化节点数据
    /// </summary>
    /// <param name="text">节点的序列化文本</param>
    /// <returns>反序列化结果，包含自定义数据和可选的位置提示</returns>
    public abstract NodeDeserializeResult Deserialize(string text);
}
#endif