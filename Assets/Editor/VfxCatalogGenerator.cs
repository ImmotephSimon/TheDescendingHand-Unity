//using System;
//using System.Collections.Generic;
//using UnityEditor;
//using UnityEngine;

//public class VfxCatalogGenerator : ICatalogBuilder
//{
//    private const string CatalogPath = "Assets/Resources/VfxCatalog.asset";

//    public bool Rebuild(CatalogAssetIndex index)
//    {
//        VfxCatalog catalog = AssetDatabase.LoadAssetAtPath<VfxCatalog>(CatalogPath);
//        bool created = catalog == null;

//        if (created)
//        {
//            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
//                AssetDatabase.CreateFolder("Assets", "Resources");

//            catalog = ScriptableObject.CreateInstance<VfxCatalog>();
//            AssetDatabase.CreateAsset(catalog, CatalogPath);
//        }

//        var records = new List<CatalogAssetRecord>();

//        foreach (CatalogAssetRecord record in index.Entries)
//        {
//            if ((record.Kind & CatalogAssetKind.VfxPrefab) == 0)
//                continue;

//            if (record.Asset is not GameObject)
//                continue;

//            records.Add(record);
//        }

//        records.Sort((a, b) => string.CompareOrdinal(a.Id, b.Id));

//        var generatedEntries = new List<VfxEntry>(records.Count);
//        foreach (CatalogAssetRecord record in records)
//        {
//            if (Enum.TryParse<VfxType>(record.Id, out var vfxType))
//            {
//                generatedEntries.Add(new VfxEntry(vfxType, (GameObject)record.Asset));
//            }
//            else
//            {
//                Debug.LogError($"[VfxCatalogGenerator] Failed to parse asset ID '{record.Id}' into VfxType.");
//            }
//        }
//        bool changed = catalog.SetGeneratedEntries(generatedEntries) || created;
//        if (changed)
//            EditorUtility.SetDirty(catalog);

//        return changed;
//    }
//}
