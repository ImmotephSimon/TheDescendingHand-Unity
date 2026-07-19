using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
[CreateAssetMenu(menuName = "Restrictions/DamageRestriction")]
public class DamageRestriction : Restriction
{
    public override bool AppliesTo(ItemDefinition item)
    {
        throw new NotImplementedException();
    }
}

