#if TOOLS
using System.Collections.Generic;
using Godot;

namespace TextualGraph.Editor.EditorNode;

public delegate void MetadataChangedEventHandler(IGraphNode source, Dictionary<string, object> meta);

/// <summary>
/// 图节点接口，定义了对话编辑器中图形节点的基本功能和行为
/// </summary>
public interface IGraphNode
{
    /// <summary>
    /// 节点元数据改变事件，用于向所有与其建立连接的节点通知元数据已改变
    /// </summary>
    event MetadataChangedEventHandler MetadataChanged;

    /// <summary>
    /// 节点位置。不需要用户手动实现，直接使用<see cref="Control.Position"/>
    /// </summary>
    Vector2 Position { get; }

    /// <summary>
    /// 节点名称。不需要用户手动实现，直接使用<see cref="Node.Name"/>
    /// </summary>
    StringName Name { get; }


    /// <summary>
    /// 节点类型，与<see cref="IGraphNodeFactory{T}.NodeName"/>一致
    /// </summary>
    string NodeType { get; }

    /// <summary>
    /// 获取节点的自定义数据，用于序列化和反序列化
    /// </summary>
    Dictionary<string, object> CustomData { get; set; }

    /// <summary>
    /// 获取自身指定输出端口的最大连接数
    /// </summary>
    /// <param name="outputPort">请求获取的自身输出端口索引</param>
    /// <returns>返回可用的最大连接数，-1表示不限制</returns>
    int GetMaxOutputConnections(long outputPort);

    /// <summary>
    /// 获取自身指定输入端口的最大连接数
    /// </summary>
    /// <param name="inputPort">请求获取的自身输入端口索引</param>
    /// <returns>返回可用的最大连接数，-1表示不限制</returns>
    int GetMaxInputConnections(long inputPort);

    /// <summary>
    /// 获取自身指定输出端口的有效输出节点名称，即这个端口能输出给谁，谁能接受这个输入
    /// </summary>
    /// <param name="outputPort">请求获取的位于自身的输出端口</param>
    /// <returns>返回所有有效的输出节点名称，返回null表示接受任何连接，返回空集合表示不接受任何连接</returns>
    HashSet<string> GetValidToNodeNamesForPort(long outputPort);

    /// <summary>
    /// 获取自身指定输入端口的有效输入节点名称，即这个端口允许谁进行输入，谁能输出到这个端口
    /// </summary>
    /// <param name="inputPort">请求获取的请求获取的位于自身的输入端口</param>
    /// <returns>返回所有有效的输入节点名称，返回null表示接受任何连接，返回空集合表示不接受任何连接</returns>
    HashSet<string> GetValidFromNodeNamesForPort(long inputPort);

    /// <summary>
    /// 当自己作为输出节点时，检查给定的输入节点以及端口是否可以建立连接
    /// </summary>
    /// <param name="input">请求建立连接的输入节点</param>
    /// <param name="inputPort">请求建立连接的输入端口</param>
    /// <param name="outputPort">输出参数，表示应该连接的位于自己的哪个输出端口</param>
    /// <returns>如果能够建立连接返回true，否则返回false</returns>
    bool CanConnectWhenIsOutput(GraphNode input, long inputPort, out long outputPort);

    /// <summary>
    /// 当自己作为输入节点时，检查给定的输出节点以及端口是否可以建立连接
    /// </summary>
    /// <param name="output">请求建立连接的输出节点</param>
    /// <param name="outputPort">请求建立连接的输出节点的输出端口</param>
    /// <param name="inputPort">输出参数，表示应该连接的位于自己的哪个输入端口</param>
    /// <returns>如果能够建立连接返回true，否则返回false</returns>
    bool CanConnectWhenIsInput(GraphNode output, long outputPort, out long inputPort);
}
#endif