using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;

[System.Serializable]
public class Item{

    public string name;
    [XmlIgnore]
    public GameObject prefab;
    public string prefabName;
    //public Vector3 posInScene = Vector3.zero; --- krijg het in korte tijd niet voor elkaar om dit goed te verwerken (wat als er meerdere items in de scene zijn) (list)
    public string category = "-";
    [Range(0, 20)]
    public int amoutOfTags = 0;
    public string tagStr = "-";
    public string description = "Description";
}

[System.Serializable]
public class ItemData {
    public List<string> tagMenu = new List<string>();
    public List<Item> data;
}

public class ItemDataBase : ScriptableObject {
    public ItemData data;    
}

/*

 public class ItemData{

    public string name;
    [XmlIgnore]
    public GameObject prefab;
    public string prefabName;
    //public Vector3 posInScene = Vector3.zero; --- krijg het in korte tijd niet voor elkaar om dit goed te verwerken (wat als er meerdere items in de scene zijn) (list)
    public string category = "-";
    [Range(0, 20)]
    public int amoutOfTags = 0;
    public string tagStr = "-";
    public string description = "Description";
}

public class ItemData { }

public class ItemDataBase : ScriptableObject {
    public List<string> tagMenu = new List<string>();
    public List<ItemData> data;
    
}



 
 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;

[System.Serializable]
public class Item{

    public string name;
    [XmlIgnore]
    public GameObject prefab;
    public string prefabName;
    //public Vector3 posInScene = Vector3.zero; --- krijg het in korte tijd niet voor elkaar om dit goed te verwerken (wat als er meerdere items in de scene zijn) (list)
    public string category = "-";
    [Range(0, 20)]
    public int amoutOfTags = 0;
    public string tagStr = "-";
    public string description = "Description";
}

ItemData {
     public List<string> tagMenu = new List<string>();
public List<ItemData> data;
    }

public class ItemDataBase : ScriptableObject {
   // public List<string> tagMenu = new List<string>();
   // public List<ItemData> data;
    public ItemData data;
}

*/
