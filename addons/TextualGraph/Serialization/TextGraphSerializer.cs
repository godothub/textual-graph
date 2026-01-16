#if TOOLS
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TextualGraph.Serialization;

/// <summary>
/// 文本图序列化器，负责将节点图数据序列化为文本格式以及从文本格式反序列化
/// </summary>
public class TextGraphSerializer
{   
    /// <summary>
    /// 存储节点类型到其对应序列化器的映射字典
    /// </summary>
    protected Dictionary<string, NodeSerializer> NodeSerializers { get; }
    
    /// <summary>
    /// 连接解析器，用于处理节点之间的连接关系
    /// </summary>
    protected ConnectionParser ConnectionParser { get; }
    
    /// <summary>
    /// 文本解析器，用于解析序列化的文本内容
    /// </summary>
    protected TextParser TextParser { get; }
    
    /// <summary>
    /// 片段写入器，用于将序列化的内容写入输出流
    /// </summary>
    protected FragmentWriter FragmentWriter { get; }

    /// <summary>
    /// 初始化<see cref="TextGraphSerializer"/>类的新实例
    /// </summary>
    /// <param name="nodeSerializers">节点序列化器的可枚举集合</param>
    /// <param name="connectionParser">连接解析器实例</param>
    /// <param name="textParser">文本解析器实例</param>
    /// <param name="fragmentWriter">片段写入器实例</param>
    public TextGraphSerializer(IEnumerable<NodeSerializer> nodeSerializers, ConnectionParser connectionParser, TextParser textParser, FragmentWriter fragmentWriter)
    {
        NodeSerializers = nodeSerializers.ToDictionary(x => x.NodeType, x => x);
        ConnectionParser = connectionParser;
        TextParser = textParser;
        FragmentWriter = fragmentWriter;
    }

    /// <summary>
    /// 初始化<see cref="TextGraphSerializer"/>类的新实例
    /// </summary>
    /// <param name="nodeSerializers">节点类型到序列化器的映射字典</param>
    /// <param name="connectionParser">连接解析器实例</param>
    /// <param name="textParser">文本解析器实例</param>
    /// <param name="fragmentWriter">片段写入器实例</param>
    public TextGraphSerializer(Dictionary<string, NodeSerializer> nodeSerializers, ConnectionParser connectionParser, TextParser textParser, FragmentWriter fragmentWriter)
    {
        NodeSerializers = nodeSerializers;
        ConnectionParser = connectionParser;
        TextParser = textParser;
        FragmentWriter = fragmentWriter;
    }

    /// <summary>
    /// 将图数据序列化为文本并写入指定的写入器
    /// </summary>
    /// <param name="writer">目标文本写入器</param>
    /// <param name="graph">要序列化的图数据</param>
    public void Serialize(TextWriter writer, GraphData graph)
    {
        // nodeId -> text
        var nodeFragments = new Dictionary<string, string>();

        foreach (var node in graph.Nodes)
        {
            if (!NodeSerializers.TryGetValue(node.NodeType, out var serializer))
                continue;
            
            if (!serializer.CanSerialize(node, graph))
                continue;


            nodeFragments[node.NodeId] = serializer.Serialize(node, graph);
        }

        var orderedFragments = ConnectionParser.Order(graph, nodeFragments);

        FragmentWriter.Begin(writer);

        for (int i = 0; i < orderedFragments.Count; i++)
        {
            FragmentWriter.WriteFragment(
                writer,
                orderedFragments[i],
                i == orderedFragments.Count - 1
            );
        }

        FragmentWriter.End(writer);
    }

    /// <summary>
    /// 从文本读取器中反序列化图数据
    /// </summary>
    /// <param name="reader">包含序列化图数据的文本读取器</param>
    /// <returns>反序列化后的图数据对象</returns>
    public GraphData Deserialize(TextReader reader)
    {
        var fragments = TextParser.Parse(reader);

        var nodes = new List<NodeData>();
        foreach (var frag in fragments)
        {
            if (!NodeSerializers.TryGetValue(frag.NodeType, out var serializer))
                continue;

            if (!serializer.CanDeserialize(frag))
                continue;

            var node = serializer.Deserialize(frag.Text);

            nodes.Add(new NodeData(
                frag.NodeId,
                frag.NodeType,
                node.PositionHint,
                node.CustomData
            ));
        }

        var connections = ConnectionParser.Restore(fragments, nodes);

        return new GraphData(nodes, connections);
    }
}
#endif