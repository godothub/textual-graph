#if TOOLS
using System.Collections.Generic;
using TextualGraph.Extension;
using Godot;
using TextualGraph.Editor.EditorNode;

namespace TextualGraph.Samples;

[Tool]
public partial class TestNode : GraphNode, IGraphNode, IGraphNodeFactory<TestNode>
{
    public static string NodeName => "dialogue";
    public static string PrefabFilePath => "res://addons/TextualGraph/Samples/Node/TestNode.tscn";

    public string NodeType => NodeName;

    [Export]
    private SpinBox _spinBox;
    [Export]
    private TextEdit _textEdit;

    public event MetadataChangedEventHandler MetadataChanged;

    public Dictionary<string, object> CustomData
    {
        get => new()
        {
            ["id"] = (int)_spinBox.Value,
            ["text"] = _textEdit.Text
        };
        set
        {
            _spinBox.Value = (int)value["id"];
            _textEdit.Text = (string)value["text"];        
        }
    }

    public bool CanConnectWhenIsInput(GraphNode output, long outputPort, out long inputPort)
    {
        inputPort = -1;
        if (output is IGraphNode graphNode)
        {
            if (!graphNode.GetValidFromNodeNamesForPort(outputPort).Contains(NodeName))
            {
                return false;
            }

            if (output is TestNode2 or TestNode)
            {
                if (output.GetInputPortType((int)outputPort) == 0)
                {
                    inputPort = this.GetOutputPortBySlot(0) ?? 0;
                    return true;
                }
            }
        }
        return false;
    }

    public bool CanConnectWhenIsOutput(GraphNode input, long inputPort, out long outputPort)
    {
        outputPort = -1;
        if (input is IGraphNode graphNode)
        {
            if (!graphNode.GetValidFromNodeNamesForPort(inputPort).Contains(NodeName))
            {
                return false;
            }

            if (input is TestNode2 or TestNode)
            {
                if (input.GetInputPortType((int)inputPort) == 0)
                {
                    outputPort = this.GetInputPortBySlot(0) ?? 0;
                    return true;
                }
            }
        }
        return false;
    }

    public int GetMaxInputConnections(long inputPort)
    {
        return 3;
    }

    public int GetMaxOutputConnections(long outputPort)
    {
        return 1;
    }

    public HashSet<string> GetValidFromNodeNamesForPort(long inputPort)
    {
        if (this.GetInputPortBySlot(0) == inputPort)
        {
            return ["dialogue", "choice"];
        }
        return [];
    }

    public HashSet<string> GetValidToNodeNamesForPort(long outputPort)
    {
        if (this.GetOutputPortBySlot(0) == outputPort)
        {
            return ["dialogue", "choice"];
        }
        return [];
    }
}

#endif