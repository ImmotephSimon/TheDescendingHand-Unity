using UnityEditor.IMGUI.Controls;
using System.Collections.Generic;

public class GameTagDropdown : AdvancedDropdown
{
    private readonly string[] _tags;
    private readonly System.Action<string> _onSelected;

    public GameTagDropdown(AdvancedDropdownState state, string[] tags, System.Action<string> onSelected) : base(state)
    {
        _tags = tags;
        _onSelected = onSelected;
        minimumSize = new UnityEngine.Vector2(250, 300);
    }

    protected override AdvancedDropdownItem BuildRoot()
    {
        var root = new AdvancedDropdownItem("Select Tag");

        foreach (var tag in _tags)
        {
            var item = new AdvancedDropdownItem(tag) { name = tag };
            root.AddChild(item);
        }

        return root;
    }

    protected override void ItemSelected(AdvancedDropdownItem item)
    {
        base.ItemSelected(item);
        _onSelected?.Invoke(item.name);
    }
}