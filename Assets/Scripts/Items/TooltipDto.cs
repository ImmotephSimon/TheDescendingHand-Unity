using System;
using System.Collections.Generic;

public enum TooltipLayout : byte { Equipment, Card, Currency }

public readonly struct TooltipDto
{
    public readonly Guid Id;
    public readonly TooltipLayout Layout;
    public readonly string Title;
    public readonly List<string> Header, Body, Footer;

    public TooltipDto(
        Guid id,
        TooltipLayout layout,
        string title,
        List<string> header = null,
        List<string> body = null,
        List<string> footer = null)
    {
        if (id == Guid.Empty) throw new ArgumentException("Id required", nameof(id));
        if (string.IsNullOrEmpty(title)) throw new ArgumentException("Title required", nameof(title));

        Id = id;
        Layout = layout;
        Title = title;
        Header = header;
        Body = body;
        Footer = footer;
    }

    public bool IsValid => Id != Guid.Empty;
}