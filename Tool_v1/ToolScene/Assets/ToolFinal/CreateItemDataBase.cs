using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CreateItemDataBase {
   // [MenuItem("Assets/Create/Item Data Base")]
    public static ItemDataBase Create(string name){
        ItemDataBase asset = ScriptableObject.CreateInstance<ItemDataBase>();
        AssetDatabase.CreateFolder("Assets", name.ToString());
        AssetDatabase.CreateAsset(asset, "Assets/" + name + "/" + name+ ".asset");
        AssetDatabase.SaveAssets();
        return asset;
    }

}
