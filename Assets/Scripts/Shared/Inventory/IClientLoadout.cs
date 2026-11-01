using System;
using System.Collections.Generic;

public interface IClientLoadout
{
    IEnumerable<object> Equipped { get; }

    event Action OnLoadoutChanged;
}