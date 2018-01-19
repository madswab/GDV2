using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

[ExecuteInEditMode]
public class XMLManager : MonoBehaviour {
    /* static funtie nu
    public ItemDataBase obj;
    public ItemData save;
    private string path;
    
    void Awake(){      
        path = Application.dataPath + "/" + obj.name + "/" + "ItemDataBaseXML.xml";
        save = obj.data;
    }

    void Update(){
        if (Input.GetKeyDown(KeyCode.W)){
            SaveToXMl(path);
        }
        if (Input.GetKeyDown(KeyCode.E)){
            obj.data = LoadFromXML(path);
        }
    }
    */
    public static void SaveToXMl(string path, ItemData save){
        var serializer = new XmlSerializer(typeof(ItemData));
        using (var stream = new FileStream(path, FileMode.Create)){
            serializer.Serialize(stream, save);
        }
        //Debug.Log("Data Saved To XML!");
    }

    public static ItemData LoadFromXML(string path){
        var serializer = new XmlSerializer(typeof(ItemData));
        using (var stream = new FileStream(path, FileMode.Open)){
            //Debug.Log("Data Loaded From XML!");
            return serializer.Deserialize(stream) as ItemData;
        } 

    }









}

/*
    public static XMLManager ins;                   //singleton    
    public ItemDataBase IDB;                        //list of items

    void Awake () {
        ins = this;
	}
    public void SaveItems(){                        //save
        XmlSerializer serializer = new XmlSerializer(typeof(ItemDataBase));
        FileStream stream = new FileStream(Application.dataPath + "/" + IDB.name + "/" + "ItemDataBaseXML.xml", FileMode.Create);
        serializer.Serialize(stream, IDB);
        stream.Close();
    }
    public void OpenItems(){                        //open
        XmlSerializer serializer = new XmlSerializer(typeof(ItemDataBase));
        FileStream stream = new FileStream(Application.dataPath + "/" + IDB.name + "/" + "ItemDataBaseXML.xml", FileMode.Open);
        IDB = serializer.Deserialize(stream) as ItemDataBase;
        stream.Close();
    }
}
*/