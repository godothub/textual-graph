#if TOOLS
using System.Collections.Generic;

namespace TextualGraph.Editor.EditorNode;

/// <summary>
/// 为图形节点元数据变化提供监听接口
/// </summary>
public interface IGraphNodeMetadataListener
{
    /// <summary>
    /// 当连接节点的元数据发生改变时调用
    /// </summary>
    /// <param name="source">源节点</param>
    /// <param name="meta">包含更改后的元数据的只读字典，键为字符串类型的元数据名称，值为元数据对象</param>
    /// <param name="isOutput">指示元数据发生变化的节点是否处于自身节点的输出位置</param>
    /// <param name="port">自身与变化节点所连接的端口索引</param>
    void OnConnectionNodeMetadataChanged(
        IGraphNode source,
        IReadOnlyDictionary<string, object> meta,
        bool isOutput,
        long port
    );
}
#endif