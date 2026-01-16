#if TOOLS
using System.Collections.Generic;
using Godot;
using TextualGraph.Editor.EditorNode;

[Tool]
public partial class TestNode3 : GraphNode, IGraphNode, IGraphNodeFactory<TestNode3>, IGraphNodeMetadataListener
{
	[Export]
	private SpinBox _spinBox;

	/// <inheritdoc />
	public event MetadataChangedEventHandler MetadataChanged;

	/// <inheritdoc />
	public static string NodeName => "test_3";

	/// <inheritdoc />
	public static string PrefabFilePath => "res://addons/TextualGraph/Samples/Node/TestNode3.tscn";

	/// <inheritdoc />
	public string NodeType => NodeName;

	/// <inheritdoc />
	public Dictionary<string, object> CustomData { get; set; }

    public override void _Ready()
    {
        _spinBox.ValueChanged += (value) =>
        {
			MetadataChanged?.Invoke(this, new Dictionary<string, object>()
			{
				{ "spin_box", value }
			});
        };
    }


	/// <inheritdoc />
	public bool CanConnectWhenIsInput(GraphNode output, long outputPort, out long inputPort)
	{
		inputPort = -1;
		if (output is TestNode3)
		{
			inputPort = 0;
			return true;
		}
		return false;
	}

	/// <inheritdoc />
	public bool CanConnectWhenIsOutput(GraphNode input, long inputPort, out long outputPort)
	{
		outputPort = -1;
		if (input is TestNode3)
        {
			outputPort = 0;
			return true;
        }
		return false;
	}

	/// <inheritdoc />
	public int GetMaxInputConnections(long inputPort)
	{
		return 1;
	}

	/// <inheritdoc />
	public int GetMaxOutputConnections(long outputPort)
	{
		return 1;
	}

	/// <inheritdoc />
	public HashSet<string> GetValidFromNodeNamesForPort(long inputPort)
	{
		return ["test_3"];
	}

	/// <inheritdoc />
	public HashSet<string> GetValidToNodeNamesForPort(long outputPort)
	{
		return ["test_3"];
	}

	public void OnConnectionNodeMetadataChanged(IGraphNode source, IReadOnlyDictionary<string, object> meta, bool isOutput, long port)
	{
		if (source is TestNode3)
        {
            _spinBox.Value = (double)meta["spin_box"] + (isOutput ? 1 : -1);
        }

	}

}
#endif
