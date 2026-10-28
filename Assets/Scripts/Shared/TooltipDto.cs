using System;
using System.Collections.Generic;

public enum TooltipLayout : byte { Equipment, Card, Currency }

public readonly struct TooltipDto
{
    public readonly SpriteString Icon;
    public readonly TooltipLayout Layout;
    public readonly string Title;
    public readonly List<string> Header, Body, Footer;

    public TooltipDto(
        SpriteString icon,
        TooltipLayout layout,
        string title,
        List<string> header = null,
        List<string> body = null,
        List<string> footer = null)
    {
        if (string.IsNullOrEmpty(title)) throw new ArgumentException("Title required", nameof(title));

        Icon = icon;
        Layout = layout;
        Title = title;
        Header = header;
        Body = body;
        Footer = footer;
    }
}