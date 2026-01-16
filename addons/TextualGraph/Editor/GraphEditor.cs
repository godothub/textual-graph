#if TOOLS
using System.Collections.Generic;
using System.Linq;
using TextualGraph.Editor.EditorNode;
using TextualGraph.Serialization;
using Godot;

namespace TextualGraph.Editor;

[Tool]
public sealed partial class GraphEditor : GraphEdit, ISerializationListener
{
	// from = output, to = input

	[Export]
	private NodeSelectWindow _nodeSelectWindow;

	[Export]
	private NodeMouseRightWindow _nodeMouseRightWindow;

	private Vector2 _clickPosition;

	private GraphElement _lastSelectedNode;

	private HashSet<GraphElement> _selectedNodes = [];

	private List<GraphNode> _nodes = [];

	private StringName _pendingNodeName;
	private int _pendingPort;
	private bool _pendingIsFrom; // true = from empty，false = to empty


	public override void _EnterTree()
	{
		_nodeSelectWindow.OnNodeSelected += OnNodeSelected;
		_nodeMouseRightWindow.ActionButtonPressed += OnRightActionPressed;
	}

    public override void _Ready()
    {
		PopupRequest += OnPopupRequest;
        NodeSelected += OnNodeSelected;
		NodeDeselected += OnNodeDeselected;
		ConnectionDragStarted += OnConnectionDragStarted;
		ConnectionFromEmpty += OnConnectionFromEmpty;
		ConnectionToEmpty += OnConnectionToEmpty;
		DeleteNodesRequest += OnDeleteNodesRequest;
		ConnectionRequest += OnConnectionRequest;
		DisconnectionRequest += OnDisconnectionRequest;
		_nodeSelectWindow.PopupHide += OnPopupHide;
    }

    public override void _ExitTree()
    {
		_nodeSelectWindow.OnNodeSelected -= OnNodeSelected;
		_nodeMouseRightWindow.ActionButtonPressed -= OnRightActionPressed;
    }


	public override bool _IsNodeHoverValid(StringName outputNode, int outputPort, StringName toNode, int toPort)
	{
		// 禁止连接自身
		return outputNode != toNode;
	}

	private void OnConnectionDragStarted(StringName nodeName, long port, bool isOutput)
	{
		var node = _nodes.FirstOrDefault(n => n.Name == nodeName);
		if (node is not IGraphNode graphNode)
			return;

		if (isOutput)
		{
			int max = graphNode.GetMaxOutputConnections(port);
			if (max >= 0)
				EnsureOutputCapacity(nodeName, (int)port, max);
		}
		else
		{
			int max = graphNode.GetMaxInputConnections(port);
			if (max >= 0)
				EnsureInputCapacity(nodeName, (int)port, max);
		}
	}

	private void EnsureOutputCapacity(StringName outputNodeName, int outputPort, int maxConnections)
	{
		var connections = GetConnectionList()
			.Where(dict => dict["from_node"].AsStringName() == outputNodeName &&
						dict["from_port"].AsInt32() == outputPort)
			.ToList();

		if (connections.Count >= maxConnections)
		{
			var last = connections.Last();
			DisconnectNode(
				last["from_node"].AsStringName(),
				last["from_port"].AsInt32(),
				last["to_node"].AsStringName(),
				last["to_port"].AsInt32()
			);
		}
	}

	private void EnsureInputCapacity(StringName inputNodeName, int inputPort, int maxConnections)
	{
		var connections = GetConnectionList()
			.Where(dict => dict["to_node"].AsStringName() == inputNodeName &&
						dict["to_port"].AsInt32() == inputPort)
			.ToList();

		if (connections.Count >= maxConnections)
		{
			var last = connections.Last();
			DisconnectNode(
				last["from_node"].AsStringName(),
				last["from_port"].AsInt32(),
				last["to_node"].AsStringName(),
				last["to_port"].AsInt32()
			);
		}
	}

	

	private void OnDisconnectionRequest(StringName outputNodeName, long outputPort, StringName inputNodeName, long inputPort)
	{
		DisconnectNode(outputNodeName, (int)outputPort, inputNodeName, (int)inputPort);
	}
	
	private void OnConnectionRequest(StringName outputNodeName, long outputPort, StringName inputNodeName, long inputPort)
	{
		var outputNode = _nodes.FirstOrDefault(n => n.Name == outputNodeName);
		var inputNode = _nodes.FirstOrDefault(n => n.Name == inputNodeName);

		var outputGraph = outputNode as IGraphNode;
		var inputGraph = inputNode as IGraphNode;

		// 语义校验
		if (!outputGraph.CanConnectWhenIsOutput(inputNode, inputPort, out var acceptOutputPort))
			return;

		if (acceptOutputPort != outputPort)
			return;
		

		if (!inputGraph.CanConnectWhenIsInput(outputNode, outputPort, out var acceptInputPort))
			return;

		if (acceptInputPort != inputPort)
			return;
	

		// 输出端容量检查
		var outputMax = outputGraph.GetMaxOutputConnections(outputPort);
		if (outputMax >= 0)
			EnsureOutputCapacity(outputNodeName, (int)outputPort, outputMax);
	

        // 输入端容量检查
		var inputMax = inputGraph.GetMaxInputConnections(inputPort);
		if (inputMax >= 0)
			EnsureInputCapacity(inputNodeName, (int)inputPort, inputMax);
	

        ConnectNode(outputNodeName, (int)outputPort, inputNodeName, (int)inputPort);
	}


	private void OnPopupHide()
	{
		if (!_nodeSelectWindow.IsOneshot)
			_pendingIsFrom = false;
    }

	private void OnDeleteNodesRequest(Godot.Collections.Array<StringName> nodeNames)
	{
		_selectedNodes.RemoveWhere(n => nodeNames.Contains(n.Name));
		_nodes.RemoveAll(n =>
		{
			if (nodeNames.Contains(n.Name))
			{
				RemoveChild(n);
				n.QueueFree();
				return true;
			}
			return false;
		});
		_lastSelectedNode = null;
	}

	private void OnConnectionFromEmpty(StringName inputNodeName, long inputPort, Vector2 releasePos)
	{
		_pendingIsFrom = true;
		_pendingNodeName = inputNodeName;
		_pendingPort = (int)inputPort;

		var inputNode = _nodes.FirstOrDefault(n => n.Name == inputNodeName);
		_clickPosition = releasePos;
		ShowNodeSelectWindow(
			releasePos,
			((IGraphNode)inputNode).GetValidFromNodeNamesForPort(inputPort),
			oneshot: true
		);
	}
	private void OnConnectionToEmpty(StringName outputNodeName, long outputPort, Vector2 releasePos)
    {
		_pendingIsFrom = false;
		_pendingNodeName = outputNodeName;
		_pendingPort = (int)outputPort;

		var outputNode = _nodes.FirstOrDefault(n => n.Name == outputNodeName);
		_clickPosition = releasePos;
		ShowNodeSelectWindow(
			releasePos,
			((IGraphNode)outputNode).GetValidToNodeNamesForPort(outputPort),
			oneshot: true
		);
    }
	private void OnRightActionPressed(NodeRightAction action)
	{
		switch (action)
		{
			case NodeRightAction.Delete:
				foreach (var node in _selectedNodes.ToList())
				{
					if (!IsInstanceValid(node))
						continue;

					RemoveChild(node);
					_selectedNodes.Remove(node);
					if (node is GraphNode graph)
						_nodes.Remove(graph);

					node.QueueFree();
					
				}

				_lastSelectedNode = null;
				break;
		}
	}

	private void OnNodeSelected(Node node)
	{
		_lastSelectedNode = (GraphElement)node;
		_selectedNodes.Add(_lastSelectedNode);
	}

	private void OnNodeDeselected(Node node)
	{
		_selectedNodes.Remove((GraphElement)node);
		_lastSelectedNode = _selectedNodes.LastOrDefault();
	}

	private void OnNodeSelected(string nodeName)
	{
		var node = GraphNodeFactory.Create(nodeName);
		if (node == null)
			return;

		Vector2 canvasPos = (_clickPosition + ScrollOffset) / Zoom;
		// 从输入端拖到空白创建的时候要向左偏移，此时是位置刚好是输出端
		node.PositionOffset = canvasPos + new Vector2(_pendingIsFrom ? -node.Size.X : 0, 0);
		_nodes.Add(node);
		
		var newNode = (IGraphNode)node;
		newNode.MetadataChanged += OnNodeMetadataChanged;

		AddChild(node);


		// 从输入输出端到空白处创建节点时，检查是否能自动连接
		if (_pendingNodeName != null)
		{
			var pendingNode = _nodes.FirstOrDefault(n => n.Name == _pendingNodeName);
			if (pendingNode == null)
				goto resetPending;

			if (_pendingIsFrom)
			{
				if (newNode.CanConnectWhenIsOutput(pendingNode, _pendingPort, out var outputPort))
				{
					EmitSignalConnectionRequest(
						node.Name,
						(int)outputPort,
						pendingNode.Name,
						_pendingPort
					);
				}
			}
			else
			{
				if (newNode.CanConnectWhenIsInput(pendingNode, _pendingPort, out var inputPort))
				{
					EmitSignalConnectionRequest(
						pendingNode.Name,
						_pendingPort,
						node.Name,
						(int)inputPort
					);
				}
			}
		}

		resetPending:
		_pendingIsFrom = false;
		_pendingNodeName = null;
		_pendingPort = 0;
	}

	private void OnPopupRequest(Vector2 pos)
	{
		_nodeMouseRightWindow.Hide();
		_nodeSelectWindow.Hide();

		_clickPosition = pos;

		if (_lastSelectedNode == null)
		{
			ShowNodeSelectWindow(pos);
		}
		else
		{
			ShowNodeMouseRightWindow(pos);
		}
	}

	private void ShowNodeMouseRightWindow(Vector2 pos)
	{
		SetPopPosition(_nodeMouseRightWindow, pos);
		
		_nodeMouseRightWindow.Popup();
	}

	private void ShowNodeSelectWindow(Vector2 pos, HashSet<string> availableButtons = null, bool oneshot = false)
	{		
		SetPopPosition(_nodeSelectWindow, pos);
		
		_nodeSelectWindow.ShowSelectMenu(availableButtons, oneshot);
	}

	private void SetPopPosition(Popup pop, Vector2 pos)
	{
		Vector2 globalMousePos = GetScreenPosition() + pos;

		var windowSize = _nodeSelectWindow.Size;
		var screenSize = DisplayServer.ScreenGetSize();

		float maxX = screenSize.X - windowSize.X;
		float maxY = screenSize.Y - windowSize.Y;

		Vector2I clampedPos = new(
			Mathf.Clamp((int)globalMousePos.X, 0, (int)maxX),
			Mathf.Clamp((int)globalMousePos.Y, 0, (int)maxY)
		);

		pop.Position = clampedPos;
	}

	private void OnNodeMetadataChanged(IGraphNode source, Dictionary<string, object> meta)
	{
		if (!IsNodeHasConnection((GraphNode)source))
			return;
		
		
		var connections = GetConnectionListFromNode(source.Name);
		foreach (var connection in connections)
		{
			var output = connection["from_node"].AsString();
			var input = connection["to_node"].AsString();
			var outputPort = connection["from_port"].AsInt32();
			var inputPort = connection["to_port"].AsInt32();

			if (!string.IsNullOrEmpty(output) && output != source.Name)
			{
				var outputNode = _nodes.FirstOrDefault(n => n.Name == output);
				if (outputNode is IGraphNodeMetadataListener listener)
				{
					listener.OnConnectionNodeMetadataChanged(source, meta, isOutput: false, port: inputPort);
				}
			}

			if (!string.IsNullOrEmpty(input) && input != source.Name)
			{
				var inputNode = _nodes.FirstOrDefault(n => n.Name == input);
				if (inputNode is IGraphNodeMetadataListener listener)
				{
					listener.OnConnectionNodeMetadataChanged(source, meta, isOutput: true, port: outputPort);
				}
			}
		}
	}
	
	private bool IsNodeHasConnection(GraphNode node)    
        => Connections.Any(c => c["from_node"].AsString() == node.Name || c["to_node"].AsString() == node.Name);
    

	/// <summary>
    /// 获取编辑器中的节点图数据
    /// </summary>
    /// <returns>节点图数据，包含了编辑器中所有节点连接信息</returns>
	public GraphData GetGraphData()
	{
		var nodes = GetChildren()
				.OfType<IGraphNode>()
				.Select(n => new NodeData(
					n.Name,
					n.NodeType,
					n.Position,
					n.CustomData
				))
				.ToList();

		var connections = Connections
				.Select(c => new ConnectionData(
					c["from_node"].AsString(),
					c["from_port"].AsInt32(),
					c["to_node"].AsString(),
					c["to_port"].AsInt32()
				))
				.ToList();

		return new(nodes, connections);
	}

	/// <summary>
    /// 从图数据重建节点图
    /// </summary>
    /// <param name="data">图数据</param>
	public void Restore(GraphData data)
	{
		_nodes.Clear();
		_selectedNodes.Clear();

		foreach (var node in GetChildren().OfType<GraphNode>())
		{
			RemoveChild(node);
			node.QueueFree();
		}

		var nodes = data.Nodes;
		var connections = data.Connections;

		var nodeIdToName = new Dictionary<string, string>();
		var needSort = new List<GraphNode>();

		foreach (var node in nodes)
		{
			var newNode = GraphNodeFactory.Create(node.NodeType);
			if (newNode == null)
				continue;

			var name = $"{node.NodeType}_{node.NodeId}";
			newNode.Name = name;
			nodeIdToName[node.NodeId] = name;

			if (node.Position.HasValue)
			{
				newNode.PositionOffset = node.Position.Value;
			}
			else
			{
				needSort.Add(newNode);
			}
			((IGraphNode)newNode).CustomData = node.CustomData;
			AddChild(newNode);
			_nodes.Add(newNode);
		}

		foreach (var connection in connections)
		{
			if (!nodeIdToName.TryGetValue(connection.OutputNodeId, out var outputNodeName) ||
				!nodeIdToName.TryGetValue(connection.InputNodeId, out var inputNodeName))
				continue;
			ConnectNode(outputNodeName, connection.OutputPort, inputNodeName, connection.InputPort);
		}

		needSort.ForEach(n => n.Selected = true);
		ArrangeNodes();
	}
	
    public void OnBeforeSerialize()
	{        
	
    }

    public void OnAfterDeserialize()
	{
		if (!EditorInterface.Singleton.IsPluginEnabled("TextualGraph"))
            return;

		// 重载程序集后需要重新连接事件
		_nodeSelectWindow.OnNodeSelected += OnNodeSelected;
		_nodeMouseRightWindow.ActionButtonPressed += OnRightActionPressed;

		// 重新收集子节点
		_nodes = GetChildren().OfType<GraphNode>().ToList();
		_nodes.ForEach(n => ((IGraphNode)n).MetadataChanged += OnNodeMetadataChanged);
		_selectedNodes = _nodes.Cast<GraphElement>().Where(n => n.IsSelected()).ToHashSet();
		_lastSelectedNode = _selectedNodes.LastOrDefault();
    }

}
#endif
