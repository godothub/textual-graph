# TextualGraph 节点图编辑器


## 简介
TextualGraph 是一个在 Godot 编辑器中可视化编辑节点图并支持从文本导入和导出为文本的编辑器插件。

## 功能
- 在编辑器中以图形化的方式编辑需要转换为文本的数据文件
- 支持根据指定的序列化规则导出/导入节点图
- 支持自定义序列化格式
- 编辑器热重载，修改解析器/序列化器后只需重新编译项目，无需重新启用插件即可将新实现应用于插件

## 安装
1. 将整个插件文件夹复制到`addons`目录中
2. 编译项目
3. 在 Godot 编辑器中启用插件
4. 启用后，插件将自动注册所有插件拓展类型（自定义节点，解析器，序列化器）

## 配置说明

### 节点配置

[nodes.json](addons/TextualGraph/Config/nodes.json)
```json
[
	{
		"name" : "dialogue",
		"display_name" : "普通对话节点"
	},
	{
		"name" : "choice",
		"display_name" : "选择节点"
	}
]
```
- `name` 字段为节点的唯一类型名称，对应自定义节点实现的 [`IGraphNode.NodeType`](addons/TextualGraph/Editor/EditorNode/IGraphNode.cs#L26) 和[`IGraphNodeFactory<T>.NodeName`](addons/TextualGraph//Editor/EditorNode/IGraphNodeFactory.cs#L17)属性

- `display_name` 字段该类型的节点要在节点选择菜单上显示的文本

实现了新的自定义节点后，需要往此处添加对应配置，否则不会自动注册

每个在此配置文件中定义的节点类型都应当有一个对应的 [`NodeSerializer`](addons/TextualGraph/Serialization/NodeSerializer.cs) 实现，否则插件会自动为该节点类型分配一个[空的序列化器（`NullNodeSerializer`）](addons/TextualGraph/Serialization/NullNodeSerializer.cs)，导致该节点在导出时不被包含在序列化输出中。

这意味着虽然节点可以在编辑器中正常使用，但在保存到文本文件时会被忽略。

这种机制也可被有意利用，当您希望某些节点（如辅助节点、调试节点或临时标记）仅在编辑器中存在而不需要出现在最终导出的文本中时，可以故意不为它们提供序列化器实现。

### 导出文件扩展名配置

[export_file_extensions.json](addons/TextualGraph/Config/export_file_extensions.json)

```json
[
	"*.txt;文本文件",
    "*.json;JSON源文件"
]
```

每一个元素为导出时支持的文件类型，但当前框架仅支持同时使用一种自定义格式序列化节点图，因此这里最好只填写1~2个元素，第一个元素建议使用默认的txt文本文件，第二个元素则取决于自定义序列化的实现

### 序列化配置

[serialization.json](addons/TextualGraph/Config/serialization.json)
```json
{
	"connection_parser" : "json",
	"text_parser" : "json_array",
	"fragment_writer": "json_array",
	"allow_file_extensions":["*.json;JSON源文件"]
}
```

- `connection_parser` 字段为处理节点图连接信息的序列化器ID，对应于[`ConnectionParser.Id`](addons/TextualGraph/Serialization/ConnectionParser.cs#L14)，配置错误将无法导出导入文本文件

- `text_parser` 字段为从文本文件提取语义并解析节点片段的文本解析器ID，对应于[`TextParser.Id`](addons/TextualGraph/Serialization/TextParser.cs#L15)

- `fragment_writer` 字段为将文本片段写入文件的片段写入器ID，对应于[`FragmentWriter.Id`](addons/TextualGraph/Serialization/FragmentWriter.cs#L14)

- `allow_file_extensions` 数组为导入时支持的文本文件类型，建议与序列化配置所支持的格式一致



## 自定义速览

序列化和节点的实现示例见[Samples目录](addons/TextualGraph/Samples)

添加自定义节点需实现 [`IGraphNodeFactory<T>`](addons/TextualGraph//Editor/EditorNode/IGraphNodeFactory.cs) 与 [`IGraphNode`](addons/TextualGraph/Editor/EditorNode/IGraphNode.cs)接口，存在于节点配置中的新节点实现类会在插件启用时编译程序集后自动注册

除此之外，需要为每个自定义节点实现序列化器 [`NodeSerializer`](addons/TextualGraph/Serialization/NodeSerializer.cs)，输出的格式应与其他序列化器组件匹配。存在于节点配置中对应节点的序列化器在编译后自动注册。

>	**注意：** 如果没有为节点实现特定的序列化器，插件会自动为未实现序列化器的节点类型创建一个[空的序列化器（`NullNodeSerializer`）](addons/TextualGraph/Serialization/NullNodeSerializer.cs)，使得这些节点虽然可以在编辑器中使用，但不会被序列化到输出文件中。
>
>	这种机制也可以被有意使用，当您希望某些节点类型（如辅助节点、临时标记节点等）仅在编辑器中可见而不影响最终输出时，可以故意不为其提供序列化器实现。

实现 [`FragmentWriter`](addons/TextualGraph/Serialization/FragmentWriter.cs) 来定义在序列化时如何写入文件。在导入时该序列化器组件不是必须的。编译后自动注册。

实现 [`ConnectionParser`](addons/TextualGraph/Serialization/ConnectionParser.cs) 在处理序列化时对节点文本片段进行排序，来决定文本的写入顺序，以及从文本恢复节点图时根据 [`TextParser`](addons/TextualGraph/Serialization/TextParser.cs) 提取的语义片段以及 [`NodeSerializer`](addons/TextualGraph/Serialization/NodeSerializer.cs) 反序列化后的数据来恢复连接信息。编译后自动注册。

实现 [`TextParser`](addons/TextualGraph/Serialization/TextParser.cs) 在导入文本文件时从文本中提取语义，解析为节点片段列表，它将为 [`ConnectionParser`](addons/TextualGraph/Serialization/ConnectionParser.cs) 恢复连接信息提供上下文。在导出时该序列化器组件不是必须的。编译后自动注册。

如果需要监听连接节点的元数据变化，可实现 [`IGraphNodeMetadataListener`](addons/TextualGraph/Editor/EditorNode/IGraphNodeMetadataListener.cs) 接口。当连接节点的元数据发生变更时，会触发 [OnConnectionNodeMetadataChanged](addons/TextualGraph/Editor/EditorNode/IGraphNodeMetadataListener.cs#L22) 方法，使当前节点能够相应地更新自己的状态。例如，[TestNode3](addons/TextualGraph/Samples/TestNode3.cs) 节点就实现了此接口，用于同步连接节点的 SpinBox 值并进行相应的调整。

如果需要在连接节点被删除时做出反应，请实现 [INodeLifecycleListener](addons/TextualGraph/Editor/EditorNode/INodeLifecycleListener.cs) 接口

如果需要节点再在反序列化后做出反应，请实现 [IDeserializeListener](addons/TextualGraph/Editor/EditorNode/IDeserializeListener.cs) 接口

如果需要在节点接受连接和断开连接时做出反应，请实现 [IConnectionListener](addons/TextualGraph/Editor/EditorNode/IConnectionListener.cs) 接口

如果需要节点请求画布移动视图到另一节点，请实现 [ICanvasMoveRequester](addons/TextualGraph/Editor/EditorNode/ICanvasMoveRequester.cs) 接口

## 已知 BUG

- 在启用插件时，因不明原因，每次编译程序集Godot都会创建两个[`NodeSelectWindow`](addons/TextualGraph/Editor/NodeSelectWindow.cs)类的新实例作为孤立节点，插件已确保其在每次编译后不会驻留在内存并在检测到其为孤立节点时[`执行了清理`](addons/TextualGraph/Editor/NodeSelectWindow.cs#L147)

	执行清理时将会收到如下消息
	```
	"[NodeSelectWindow] The current instance is not in the tree, and the instance has been released."
	```

## 协议

`MIT`
