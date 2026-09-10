#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using System.Linq;

public class PassiveTreeEditorWindow : EditorWindow
{
    private PassiveTreeData _data;

    private Vector2 _pan;
    private PassiveNode _selectedNode;
    private PassiveNode _draggedNode;
    private Vector2 _dragOffset;

    private PassiveNode _connectFromNode;

    private const float NodeWidth = 140f;
    private const float NodeHeight = 70f;
    private const float InspectorWidth = 280f;

    [MenuItem("Tools/Passive Tree Editor")]
    private static void Open()
    {
        GetWindow<PassiveTreeEditorWindow>("Passive Tree");
    }

    private void OnGUI()
    {
        DrawToolbar();

        if (_data == null)
            return;

        Rect canvas = new Rect(
            0f,
            EditorGUIUtility.singleLineHeight + 4f,
            position.width - InspectorWidth,
            position.height);

        Rect inspector = new Rect(
            position.width - InspectorWidth,
            0f,
            InspectorWidth,
            position.height);

        DrawCanvas(canvas);
        DrawInspector(inspector);
        HandleCanvasInput(canvas);
    }

    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        _data = (PassiveTreeData)EditorGUILayout.ObjectField(
            _data,
            typeof(PassiveTreeData),
            false,
            GUILayout.Width(300f));

        if (GUILayout.Button("Frame All", EditorStyles.toolbarButton))
            FrameAll();

        // Explicit Save Button
        if (GUILayout.Button("Save Data", EditorStyles.toolbarButton))
        {
            if (_data != null)
            {
                EditorUtility.SetDirty(_data);
                AssetDatabase.SaveAssets();
            }
        }

        GUILayout.FlexibleSpace();

        GUILayout.Label(
            _connectFromNode != null
                ? $"Shift+click a node to connect to '{_connectFromNode.Name}' (shift+click it again to cancel)"
                : "Shift+click a node, then shift+click another to connect/disconnect them",
            EditorStyles.miniLabel);

        EditorGUILayout.EndHorizontal();
    }

    private void DrawCanvas(Rect canvas)
    {
        GUI.BeginGroup(canvas);

        EditorGUI.DrawRect(
            new Rect(0f, 0f, canvas.width, canvas.height),
            new Color(0.12f, 0.12f, 0.12f));

        DrawGrid(canvas);
        DrawConnections();
        DrawNodes();

        GUI.EndGroup();
    }

    private void DrawGrid(Rect canvas)
    {
        const float gridSize = 32f;

        Handles.color = new Color(0.2f, 0.2f, 0.2f);

        float startX = Mathf.Repeat(_pan.x, gridSize);
        float startY = Mathf.Repeat(_pan.y, gridSize);

        for (float x = startX; x < canvas.width; x += gridSize)
        {
            Handles.DrawLine(
                new Vector3(x, 0f),
                new Vector3(x, canvas.height));
        }

        for (float y = startY; y < canvas.height; y += gridSize)
        {
            Handles.DrawLine(
                new Vector3(0f, y),
                new Vector3(canvas.width, y));
        }
    }

    private void DrawConnections()
    {
        Handles.color = Color.gray;

        foreach (PassiveNodeConnection connection in _data.Connections)
        {
            PassiveNode a = FindNode(connection.A);
            PassiveNode b = FindNode(connection.B);

            if (a == null || b == null)
                continue;

            Vector2 start = ToCanvasPosition(a.Position);
            Vector2 end = ToCanvasPosition(b.Position);

            Handles.DrawAAPolyLine(4f, start, end);
        }
    }

    private void DrawNodes()
    {
        foreach (PassiveNode node in _data.Nodes)
        {
            Vector2 position = ToCanvasPosition(node.Position);

            Rect rect = new Rect(
                position.x - NodeWidth * 0.5f,
                position.y - NodeHeight * 0.5f,
                NodeWidth,
                NodeHeight);

            bool selected = node == _selectedNode;
            bool pendingConnect = node == _connectFromNode;

            GUIStyle style = selected
                ? EditorStyles.selectionRect
                : EditorStyles.helpBox;

            Color prevColor = GUI.color;
            if (pendingConnect)
                GUI.color = Color.yellow;

            GUI.Box(rect, GUIContent.none, style);

            GUI.color = prevColor;

            string typeName = node.Type;

            GUI.Label(
                new Rect(
                    rect.x + 8f,
                    rect.y + 8f,
                    rect.width - 16f,
                    20f),
                node.Name,
                EditorStyles.boldLabel);

            GUI.Label(
                new Rect(
                    rect.x + 8f,
                    rect.y + 30f,
                    rect.width - 16f,
                    20f),
                typeName);
        }
    }

    private void DrawInspector(Rect rect)
    {
        GUILayout.BeginArea(rect, EditorStyles.helpBox);

        EditorGUILayout.Space(8f);

        if (_selectedNode == null)
        {
            EditorGUILayout.LabelField(
                "Select a node",
                EditorStyles.boldLabel);

            GUILayout.EndArea();
            return;
        }

        EditorGUILayout.LabelField(
            "Node",
            EditorStyles.boldLabel);

        EditorGUILayout.Space(4f);

        EditorGUI.BeginChangeCheck();

        string oldName = _selectedNode.Name;
        string newName = EditorGUILayout.DelayedTextField("Name", oldName);

        if (newName != oldName)
        {
            RenameNode(_selectedNode, oldName, newName);
        }

        _selectedNode.Type =
            EditorGUILayout.TextField(
                "Type",
                _selectedNode.Type);

        _selectedNode.Role = (PassiveNodeRole)EditorGUILayout.EnumPopup("Role", _selectedNode.Role);

        _selectedNode.Cluster =
            EditorGUILayout.TextField(
                "Cluster",
                _selectedNode.Cluster);

        EditorGUILayout.Space(8f);

        EditorGUILayout.Vector2Field(
            "Position",
            _selectedNode.Position);

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(_data);
        }

        EditorGUILayout.Space(10f);

        EditorGUILayout.LabelField(
            "Costs",
            EditorStyles.boldLabel);

        for (int i = 0; i < _selectedNode.Cost.Count; i++)
        {
            PassiveNodeCost cost = _selectedNode.Cost[i];

            EditorGUILayout.BeginHorizontal();

            cost.Attribute = new GameTag(
                EditorGUILayout.TextField(cost.Attribute?.TagId ?? string.Empty)
            );

            cost.Amount =
                EditorGUILayout.IntField(
                    cost.Amount,
                    GUILayout.Width(60f));

            if (GUILayout.Button(
                    "X",
                    GUILayout.Width(20f)))
            {
                Undo.RecordObject(
                    _data,
                    "Remove Passive Cost");

                _selectedNode.Cost.RemoveAt(i);
                EditorUtility.SetDirty(_data);

                break;
            }

            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("Add Cost"))
        {
            Undo.RecordObject(
                _data,
                "Add Passive Cost");

            _selectedNode.Cost.Add(new PassiveNodeCost());

            EditorUtility.SetDirty(_data);
        }

        GUILayout.EndArea();
    }

    private void RenameNode(PassiveNode node, string oldName, string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
        {
            Debug.LogWarning("Rename rejected: name cannot be empty.");
            return;
        }

        if (_data.Nodes.Any(n => n != node && n.Name == newName))
        {
            Debug.LogWarning($"Rename rejected: a node named '{newName}' already exists.");
            return;
        }

        Undo.RecordObject(_data, "Rename Passive Node");

        node.Name = newName;

        int connectionsUpdated = 0;
        foreach (PassiveNodeConnection connection in _data.Connections)
        {
            if (connection.A == oldName) { connection.A = newName; connectionsUpdated++; }
            if (connection.B == oldName) { connection.B = newName; connectionsUpdated++; }
        }

        int clustersUpdated = 0;
        foreach (PassiveNode other in _data.Nodes)
        {
            if (other.Cluster == oldName)
            {
                other.Cluster = newName;
                clustersUpdated++;
            }
        }

        EditorUtility.SetDirty(_data);

        Debug.Log($"Renamed '{oldName}' -> '{newName}'. Updated {connectionsUpdated} connection reference(s), {clustersUpdated} cluster reference(s).");
    }

    private void HandleConnectClick(PassiveNode node)
    {
        if (_connectFromNode == null)
        {
            _connectFromNode = node;
            Repaint();
            return;
        }

        if (_connectFromNode == node)
        {
            _connectFromNode = null; // clicked same node again, cancel
            Repaint();
            return;
        }

        ToggleConnection(_connectFromNode, node);
        _connectFromNode = null;
        Repaint();
    }

    private void ToggleConnection(PassiveNode a, PassiveNode b)
    {
        PassiveNodeConnection existing = _data.Connections.FirstOrDefault(c =>
            (c.A == a.Name && c.B == b.Name) ||
            (c.A == b.Name && c.B == a.Name));

        Undo.RecordObject(_data, existing != null ? "Remove Passive Connection" : "Add Passive Connection");

        if (existing != null)
        {
            _data.Connections.Remove(existing);
            Debug.Log($"Removed connection: '{a.Name}' <-> '{b.Name}'");
        }
        else
        {
            _data.Connections.Add(new PassiveNodeConnection { A = a.Name, B = b.Name });
            Debug.Log($"Added connection: '{a.Name}' <-> '{b.Name}'");
        }

        EditorUtility.SetDirty(_data);
    }

    private void HandleCanvasInput(Rect canvas)
    {
        Event e = Event.current;

        if (!canvas.Contains(e.mousePosition))
            return;

        Vector2 mouse = e.mousePosition - canvas.position;

        if (e.type == EventType.MouseDown)
        {
            if (e.button == 0)
            {
                PassiveNode node = GetNodeAt(mouse);

                if (e.shift)
                {
                    if (node != null)
                        HandleConnectClick(node);

                    e.Use();
                    return;
                }

                if (node != null)
                {
                    _selectedNode = node;
                    _draggedNode = node;

                    Vector2 nodePosition = ToCanvasPosition(node.Position);
                    _dragOffset = mouse - nodePosition;

                    GUIUtility.hotControl =
                        GUIUtility.GetControlID(
                            FocusType.Passive);

                    Repaint();
                    e.Use();
                }
                else
                {
                    _selectedNode = null;
                    Repaint();
                }
            }
            else if (e.button == 2)
            {
                GUIUtility.hotControl =
                    GUIUtility.GetControlID(
                        FocusType.Passive);

                _dragOffset = mouse - _pan;
                e.Use();
            }
        }

        if (e.type == EventType.MouseDrag)
        {
            if (_draggedNode != null && e.button == 0)
            {
                Undo.RecordObject(
                    _data,
                    "Move Passive Node");

                Vector2 newCanvasPosition =
                    mouse - _dragOffset;

                _draggedNode.Position =
                    FromCanvasPosition(newCanvasPosition);

                EditorUtility.SetDirty(_data);

                Repaint();
                e.Use();
            }
            else if (GUIUtility.hotControl != 0 &&
                     e.button == 2)
            {
                _pan = mouse - _dragOffset;

                Repaint();
                e.Use();
            }
        }

        if (e.type == EventType.MouseUp)
        {
            if (e.button == 0)
                _draggedNode = null;

            if (e.button == 0 || e.button == 2)
                GUIUtility.hotControl = 0;
        }

        if (e.type == EventType.ScrollWheel)
        {
            _pan -= e.delta * 2f;

            Repaint();
            e.Use();
        }
    }

    private PassiveNode GetNodeAt(Vector2 canvasPosition)
    {
        for (int i = _data.Nodes.Count - 1; i >= 0; i--)
        {
            PassiveNode node = _data.Nodes[i];

            Vector2 position =
                ToCanvasPosition(node.Position);

            Rect rect = new Rect(
                position.x - NodeWidth * 0.5f,
                position.y - NodeHeight * 0.5f,
                NodeWidth,
                NodeHeight);

            if (rect.Contains(canvasPosition))
                return node;
        }

        return null;
    }

    private PassiveNode FindNode(string name)
    {
        return _data.Nodes.FirstOrDefault(
            node => node.Name == name);
    }

    private Vector2 ToCanvasPosition(Vector2 position)
    {
        return position + _pan;
    }

    private Vector2 FromCanvasPosition(Vector2 position)
    {
        return position - _pan;
    }

    private void FrameAll()
    {
        if (_data == null || _data.Nodes.Count == 0)
            return;

        Vector2 min = _data.Nodes[0].Position;
        Vector2 max = min;

        foreach (PassiveNode node in _data.Nodes)
        {
            min = Vector2.Min(min, node.Position);
            max = Vector2.Max(max, node.Position);
        }

        Vector2 center = (min + max) * 0.5f;

        Rect canvas = new Rect(
            0f,
            EditorGUIUtility.singleLineHeight + 4f,
            position.width - InspectorWidth,
            position.height);

        _pan = canvas.center - center;

        Repaint();
    }
}

#endif