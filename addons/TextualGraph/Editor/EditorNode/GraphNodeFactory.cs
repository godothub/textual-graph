#if TOOLS
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TextualGraph.JsonHelper;
using TextualGraph.Serialization;
using Godot;

namespace TextualGraph.Editor.EditorNode;

/// <summary>
/// 图节点工厂类，用于创建和管理各种类型的图节点
/// </summary>
public static class GraphNodeFactory
{    
    private static List<Type> _allNodeTypes;
    private static Dictionary<string, Func<GraphNode>> _factories;
    private static Dictionary<string, NodeSerializer> _nodeSerializers;

    /// <summary>
    /// 创建指定类型的图节点
    /// </summary>
    /// <typeparam name="T">要创建的节点类型，必须实现<see cref="GraphNode"/>、<see cref="IGraphNode"/>和<see cref="IGraphNodeFactory{T}"/></typeparam>
    /// <returns>创建的节点实例</returns>
    public static T Create<T>()
        where T : GraphNode, IGraphNode, IGraphNodeFactory<T>, new()
    {
        return T.Create();
    }

    /// <summary>
    /// 根据节点名称创建指定类型的图节点
    /// </summary>
    /// <typeparam name="T">要创建的节点类型，必须实现<see cref="GraphNode"/>、<see cref="IGraphNode"/>和<see cref="IGraphNodeFactory{T}"/></typeparam>
    /// <param name="nodeName">节点名称</param>
    /// <returns>创建的节点实例，如果找不到对应名称的工厂则返回null</returns>
    public static T Create<T>(string nodeName)
        where T : GraphNode, IGraphNode, IGraphNodeFactory<T>, new()
    {
        if (_factories.TryGetValue(nodeName, out var factory) && factory() is T node)
            return node;

        return null;
    }

    /// <summary>
    /// 获取所有已注册节点的名称集合
    /// </summary>
    /// <returns>节点名称的哈希集合</returns>
    public static HashSet<string> GetNodeNames() => _factories?.Keys.ToHashSet() ?? [];

    /// <summary>
    /// 获取节点序列化器字典的副本
    /// </summary>
    public static Dictionary<string, NodeSerializer> NodeSerializers => _nodeSerializers.ToDictionary();

    /// <summary>
    /// 根据节点名称创建图节点
    /// </summary>
    /// <param name="nodeName">节点名称</param>
    /// <returns>创建的节点实例，如果找不到对应名称的工厂则返回null</returns>
    public static GraphNode Create(string nodeName)
    {
        if (_factories.TryGetValue(nodeName, out var factory))
            return factory();

        return null;
    }

    /// <summary>
    /// 更新节点注册信息，根据配置重新扫描和注册节点类型
    /// </summary>
    /// <param name="typeChanged">指示类型是否发生变化的布尔值，如果为true则强制刷新类型扫描</param>
    public static void UpdateRegistration(bool typeChanged = false)
    {
        var configs = ConfigReader.DeserializeNodeConfig();
        if (configs.Count == 0)
            return;

        if (_allNodeTypes == null || _nodeSerializers == null || typeChanged)
            ScanNodeTypes(true);

        _factories ??= [];

        _nodeSerializers ??= [];

        foreach (var config in configs)
        {
            // 为没有实现序列化器的节点添加占位序列化器
            if (!_nodeSerializers.ContainsKey(config.Name))
            {
                _nodeSerializers[config.Name] = new NullNodeSerializer(config.Name);
            }
        }


        if (_factories.Count > 0 && _nodeSerializers.Count > 0)
        {
            var toRemove = _factories.Keys
                .Where(k =>
                    configs.All(c => c.Name != k) ||
                    _allNodeTypes.All(t =>
                        (string)t.GetProperty("NodeName", BindingFlags.Public | BindingFlags.Static)?.GetValue(null) != k
                    )
                )
                .ToList();

            foreach (var k in toRemove)
            {
                _factories.Remove(k);
                _nodeSerializers.Remove(k);
            }
        }

        foreach (var config in configs)
        {
            if (_factories.ContainsKey(config.Name))
                continue;

            var type = _allNodeTypes.FirstOrDefault(t =>
                (string)t.GetProperty("NodeName", BindingFlags.Public | BindingFlags.Static)?.GetValue(null) == config.Name
            );

            if (type == null)
                continue;

            // 先检查类有没有实现
            var method = type.GetMethod("Create", BindingFlags.Public | BindingFlags.Static);

            if (method == null)
            {
                var iface = type.GetInterfaces()
                    .FirstOrDefault(i =>
                        i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IGraphNodeFactory<>)
                    );
                if (iface != null)
                {
                    // 使用接口默认实现
                    method = iface.GetMethod("Create", BindingFlags.Public | BindingFlags.Static);
                }
            }

            if (method == null)
                continue;


            // 包装委托
            _factories[config.Name] = () => (GraphNode)method.Invoke(null, null);            
        }
    }

    /// <summary>
    /// 清理所有已注册的节点工厂和序列化器
    /// </summary>
    public static void Clearup()
    {
        _factories = null;
        _nodeSerializers = null;
        _allNodeTypes = null;      
    }

    private static void ScanNodeTypes(bool forceRefresh = false)
    {
        if (_allNodeTypes != null && _nodeSerializers != null && !forceRefresh)
            return;

        var nonAbstractNodeTypes = typeof(GraphNodeFactory).Assembly.GetTypes()            
            .Where(t => !t.IsAbstract);

        _allNodeTypes = nonAbstractNodeTypes
                .Where(t => t.IsSubclassOf(typeof(GraphNode)))
                .Where(t =>
                    t.GetInterfaces().Any(i =>
                        i.IsGenericType &&
                        i.GetGenericTypeDefinition() == typeof(IGraphNodeFactory<>)
                ))
                .ToList();

        _nodeSerializers = nonAbstractNodeTypes
            .Where(t => t.IsSubclassOf(typeof(NodeSerializer)) && t != typeof(NullNodeSerializer))
            .Select(t => (NodeSerializer)Activator.CreateInstance(t))
            .ToDictionary(s => s.NodeType, s => s);
    }
}
#endif