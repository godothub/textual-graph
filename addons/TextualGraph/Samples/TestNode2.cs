#if TOOLS
using System.Collections.Generic;
using TextualGraph.Extension;
using Godot;
using TextualGraph.Editor.EditorNode;

namespace TextualGraph.Samples;

[Tool]
public partial class TestNode2 : GraphNode, IGraphNode, IGraphNodeFactory<TestNode2>
{
    public static string NodeName => "choice";
    public static string PrefabFilePath => "res://addons/TextualGraph/Samples/Node/TestNode2.tscn";
    public string NodeType => NodeName;

    [Export]
    private SpinBox _spinBox;

    public event MetadataChangedEventHandler MetadataChanged;

    public Dictionary<string, object> CustomData
    {
        get => new()
        {
            ["id"] = (int)_spinBox.Value
        };
        set
        {
            _spinBox.Value = (int)value["id"];
        }
    }

    public bool CanConnectWhenIsInput(GraphNode to, long toPort, out long fromPort)
    {
        fromPort = -1;
        if (to is IGraphNode graphNode)
        {
            if (!graphNode.GetValidFromNodeNamesForPort(toPort).Contains(NodeName))
            {
                return false;
            }

            if (to is TestNode2 or TestNode)
            {
                if (to.GetInputPortType((int)toPort) == 0)
                {
                    fromPort = this.GetOutputPortBySlot(0) ?? 0;
                    return true;
                }
            }
        }
        return false;
    }

    public bool CanConnectWhenIsOutput(GraphNode from, long fromPort, out long toPort)
    {
        toPort = -1;
        if (from is IGraphNode graphNode )
        {
            if (!graphNode.GetValidFromNodeNamesForPort(fromPort).Contains(NodeName))
            {
                return false;
            }

            if (from is TestNode2 or TestNode)
            {
                if (from.GetInputPortType((int)fromPort) == 0)
                {
                    toPort = this.GetInputPortBySlot(0) ?? 0;
                    return true;
                }
            }
        }
        return false;
    }

    public int GetMaxInputConnections(long inputPort)
    {
        return -1;
    }

    public int GetMaxOutputConnections(long outputPort)
    {
        return -1;
    }
    
    public HashSet<string> GetValidFromNodeNamesForPort(long inputPort)
    {
        if (this.GetInputPortBySlot(0) == inputPort)
        {
            return ["dialogue"];
        }
        return [];
    }

    public HashSet<string> GetValidToNodeNamesForPort(long outputPort)
    {
        if (this.GetOutputPortBySlot(0) == outputPort)
        {
            return ["dialogue"];
        }
        return [];
    }
}

#endif