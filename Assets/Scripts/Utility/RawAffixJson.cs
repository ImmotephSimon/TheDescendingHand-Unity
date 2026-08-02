[System.Serializable]
public struct RawDefinitionJson
{
    public string Id;
    public float BaseValue;
    public string MathOp;      // 1:1 enum match ("Multiplicative", "Added", etc.)
    public string Modifier;    // String tag identifier (e.g. "Mod.Implicit")
    public string Restriction; // Restriction identifier (e.g. "Minimum", "None")
}