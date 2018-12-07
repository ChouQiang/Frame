using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class ModelImport : AssetPostprocessor
{
    void OnPreprocessModel()
    {
        ModelImporter modelImporter = (ModelImporter)assetImporter;
        modelImporter.importMaterials = false;
    }
}
