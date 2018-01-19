using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ItemDataBaseWindow1 : EditorWindow{

    [MenuItem("Tools/Item DataBase Window1")]
    public static void ShowWindowItemDataBaseWindow1(){
        GetWindow<ItemDataBaseWindow1>("ItemDataBase");
    }

    enum Pages {Non, MakeList, Overview, Edit  };
    private Pages currenPage = Pages.Edit;

    private ItemDataBase ItemDataBaseList;
    private int viewIndex = 1;
    private string nameItemList= "ItemDatabase";
    private float previewSize = 100;
    private Vector2 scrollPosition;

    private GenericMenu itemCategoryTagMenu = new GenericMenu();
    private GenericMenu sceneItemCategoryTagMenu = new GenericMenu();

    private bool advancedOptions = true;
    private bool categorize = false;
    private bool addMenuItem = false;
    private bool delMenuItem = false;
    private string nameMenuItem = "/ for submenu";
    private List<string> itemCategory = new List<string>();

    private int rectToolbarOffsetLeft = 257;
    private int rectToolbarOffsetRight = 274;
    private bool extension = true;
    private bool showItemsInScene = true;
    private string currentCategory = "Category";
    private string currentPrefab;

    private void OnEnable(){        
        if (EditorPrefs.HasKey("ObjectPath")){
            string objectPath = EditorPrefs.GetString("ObjectPath");
            ItemDataBaseList = AssetDatabase.LoadAssetAtPath(objectPath, typeof(ItemDataBase)) as ItemDataBase;
            if (ItemDataBaseList != null){ 
                ItemDataBaseList.data = XMLManager.LoadFromXML(Application.dataPath + "/" + ItemDataBaseList.name + "/" + "ItemDataBaseXML.xml");
                FillCategoryMenu();
                LoadInPrefabs(); 
            }
        }
    }

    private void OnGUI(){
        SceneView.onSceneGUIDelegate -= RenderToolbar;
        SceneView.onSceneGUIDelegate += RenderToolbar;

        GUILayout.BeginHorizontal();
        GUILayout.Label("Inventory Item Editor", EditorStyles.boldLabel);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (ItemDataBaseList != null){
            if (GUILayout.Button("Show Database", GUILayout.ExpandWidth(true))){
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = ItemDataBaseList;
                GUILayout.EndHorizontal();
                return;
            }
            if (GUILayout.Button("Open", GUILayout.ExpandWidth(true))){
                OpenItemList(); 
            }
            if (GUILayout.Button("New", GUILayout.ExpandWidth(true))){
                if (currenPage == Pages.MakeList){
                    currenPage = Pages.Edit;
                }
                else{
                    currenPage = Pages.MakeList;
                }
            }
            if (GUILayout.Button("Load Sarah's XML", GUILayout.ExpandWidth(true))){ 
                LoadSarahsXML(); 
            }
        }
        GUILayout.EndHorizontal();

        if (ItemDataBaseList == null){
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Open Database", GUILayout.ExpandWidth(true))){
                OpenItemList();
            }
            if (GUILayout.Button("Create Database", GUILayout.ExpandWidth(true))){
                currenPage = Pages.MakeList;
            }
            GUILayout.EndHorizontal();
        }

        GUILayout.Space(20);

        switch(currenPage){
            case Pages.MakeList:
                nameItemList = EditorGUILayout.TextField("Name", nameItemList);
                if (GUILayout.Button("Enter")){
                    if (nameItemList != ""){
                        CreateNewItemList();
                    }
                }
                break;

            case Pages.Edit:
                if (ItemDataBaseList != null){
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(45); 
                    if (ItemDataBaseList.data == null)
                        Debug.Log("wtf");

                    if (ItemDataBaseList.data.data.Count > 0){
                        if (GUILayout.Button("Prev", GUILayout.ExpandWidth(false))){
                            if (viewIndex > 1)
                                viewIndex--;
                                GUI.FocusControl(null);
                        }
                        GUILayout.FlexibleSpace();
                        Texture2D preview = EditorGUILayout.ObjectField(AssetPreview.GetAssetPreview(ItemDataBaseList.data.data[viewIndex - 1].prefab), typeof(Texture2D), true, GUILayout.Width(previewSize), GUILayout.Height(previewSize)) as Texture2D;
                        GUILayout.FlexibleSpace();

                        if (GUILayout.Button("Next", GUILayout.ExpandWidth(false))){
                            if (viewIndex < ItemDataBaseList.data.data.Count){
                                viewIndex++;
                                GUI.FocusControl(null);
                            }
                        }
                        GUILayout.Space(45);
                        GUILayout.EndHorizontal();
                        GUILayout.Space(10);

                        GUILayout.BeginHorizontal();
                        viewIndex = Mathf.Clamp(EditorGUILayout.IntField("Current Item", viewIndex, GUILayout.ExpandWidth(false)), 1, ItemDataBaseList.data.data.Count);
                        Mathf.Clamp (viewIndex, 1, ItemDataBaseList.data.data.Count);
                        EditorGUILayout.LabelField("of  " + ItemDataBaseList.data.data.Count.ToString() + "  items", "", GUILayout.ExpandWidth(false));
                        GUILayout.EndHorizontal();

                        ItemDataBaseList.data.data[viewIndex - 1].name = EditorGUILayout.TextField("Name", ItemDataBaseList.data.data[viewIndex - 1].name as string);
                        EditorGUI.BeginChangeCheck();
                        if (ItemDataBaseList.data.data[viewIndex - 1].prefab != null){
                            currentPrefab = ItemDataBaseList.data.data[viewIndex - 1].prefab.name;
                            ItemDataBaseList.data.data[viewIndex - 1].prefabName = ItemDataBaseList.data.data[viewIndex - 1].prefab.name;
                        }
                        else{
                            currentPrefab = null;
                            ItemDataBaseList.data.data[viewIndex - 1].prefabName = null;
                        }
                        ItemDataBaseList.data.data[viewIndex - 1].prefab = EditorGUILayout.ObjectField("Object", ItemDataBaseList.data.data[viewIndex - 1].prefab, typeof(GameObject), false) as GameObject;
                        if (EditorGUI.EndChangeCheck()){
                            if (currentPrefab != null){
                                RemoveOldPrefabFromFolder();
                            }
                            DuplicatePrefabsSetInFolder();
                        }

                        advancedOptions = EditorGUILayout.Foldout(advancedOptions, "Advanced Options");
                        if (advancedOptions){
                            EditorGUI.indentLevel++;
                            string PrefabPath = EditorGUILayout.TextField("Prefab Path", ItemDataBaseList.data.data[viewIndex - 1].prefab?
                                                AssetDatabase.GetAssetPath(ItemDataBaseList.data.data[viewIndex - 1].prefab):"-", EditorStyles.label);

                            List<Vector3> itemPositions = new List<Vector3>();
                            itemPositions = GetItemPos();
                            for (int i = 0; i < itemPositions.Count; i++){
                                GUILayout.BeginHorizontal();
                                EditorGUILayout.LabelField("Prefab Position", GUILayout.Width(131));
                                EditorGUILayout.Vector3Field("", itemPositions[i]);
                                GUILayout.EndHorizontal();
                            }

                            GUILayout.BeginHorizontal();
                            EditorGUILayout.TextField("Amound", EditorStyles.label, GUILayout.Width(130));
                            EditorGUILayout.TextField("Categories", EditorStyles.label);
                            GUILayout.FlexibleSpace();
                            if (GUILayout.Button(" + ", EditorStyles.miniButton, GUILayout.ExpandWidth(false))){
                                    GUI.FocusControl(null);
                                    itemCategoryTagMenu.ShowAsContext();
                            }
                            if (GUILayout.Button(" - ", EditorStyles.miniButton, GUILayout.ExpandWidth(false))){
                                GUI.FocusControl(null);
                                ItemDataBaseList.data.data[viewIndex - 1].amoutOfTags = 0;
                                ItemDataBaseList.data.data[viewIndex - 1].category = "-";
                            }
                            GUILayout.EndHorizontal();

                            EditorGUI.indentLevel++;
                            for (int i = 0; i < ItemDataBaseList.data.data[viewIndex - 1].amoutOfTags; i++){
                                 GUILayout.BeginHorizontal();
                                 string cat = EditorGUILayout.TextField("Category " + (i+1), SepparateCategories(ItemDataBaseList.data.data[viewIndex - 1].category, (i)) as string, EditorStyles.label);                                
                                 if (GUILayout.Button(" - ", EditorStyles.miniButton, GUILayout.ExpandWidth(false))){
                                    GUI.FocusControl(null);
                                    if (ItemDataBaseList.data.data[viewIndex - 1].amoutOfTags > 0){
                                        ItemDataBaseList.data.data[viewIndex - 1].category = RemoveCatogoryFromString(ItemDataBaseList.data.data[viewIndex - 1].category, i);
                                        ItemDataBaseList.data.data[viewIndex - 1].amoutOfTags--;
                                    } 
                                    if (ItemDataBaseList.data.data[viewIndex - 1].amoutOfTags == 0){
                                        ItemDataBaseList.data.data[viewIndex - 1].category = "-";
                                    }
                                 }
                                 GUILayout.EndHorizontal();                    
                            }
                            EditorGUI.indentLevel--;

                            if (addMenuItem){
                                delMenuItem = false;
                                GUILayout.BeginHorizontal();
                                nameMenuItem = EditorGUILayout.TextField("Add category", nameMenuItem as string);
                                if (nameMenuItem == "" || nameMenuItem == "/ for submenu" || nameMenuItem[0].ToString() == "/" || nameMenuItem[nameMenuItem.Length - 1].ToString() == "/"){
                                    GUI.color = new Color(1, .7f, .7f, 1);
                                }
                                if (GUILayout.Button("Enter", EditorStyles.miniButton, GUILayout.ExpandWidth(false), GUILayout.Width(52))){
                                    addMenuItem = false;
                                    AddMenuItemCategoryMenu();
                                }
                                GUI.color = new Color(1,1,1,1);
                                GUILayout.EndHorizontal();
                            }
                            if (delMenuItem){
                                addMenuItem = false;
                                GUILayout.BeginHorizontal();
                                nameMenuItem = EditorGUILayout.TextField("Delete category", nameMenuItem as string);
                                if (GUILayout.Button("Enter", EditorStyles.miniButton, GUILayout.ExpandWidth(false), GUILayout.Width(52))){
                                    delMenuItem = false;
                                    DelMenuItemCategoryMenu();
                                }
                                GUILayout.EndHorizontal();
                            }
                            EditorGUI.indentLevel--;
                        }
                        GUILayout.Space(10);
                        ItemDataBaseList.data.data[viewIndex - 1].description = EditorGUILayout.TextArea(ItemDataBaseList.data.data[viewIndex - 1].description as string,GUILayout.Height(position.height / 5));
                        GUILayout.Space(10);
                    }
                    else{
                        GUILayout.Label("This Inventory List is Empty.");
                        GUILayout.EndVertical();
                    }
                    GUILayout.FlexibleSpace();
                    LowerButtons();
                }

                break;

            case Pages.Overview:
                scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(position.width), GUILayout.Height(position.height - 115));
                if (categorize){//-----------------------------------------------------------------------------------------todo Aline wanneer items verdwijnen gaat aline niet gooed
                    for (int z = 0; z < itemCategory.Count; z++){
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(itemCategory[z], GUILayout.ExpandWidth(false));
                        GUILayout.Space(10);
                        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        int a = 0;
                        for (int i = 0; i < ItemDataBaseList.data.data.Count; i++){
                            if ((i - a) * previewSize + previewSize >= position.width){
                                GUILayout.EndHorizontal();
                                GUILayout.BeginHorizontal();
                                a = i;
                            }
                            if (ItemDataBaseList.data.data[i].amoutOfTags > 0){
                                for (int b = 0; b < ItemDataBaseList.data.data[i].amoutOfTags; b++){
                                    if (FirstCategoryFork(SepparateCategories(ItemDataBaseList.data.data[i].category, b)) == itemCategory[z]){
                                        if (GUILayout.Button(AssetPreview.GetAssetPreview(ItemDataBaseList.data.data[i].prefab), GUILayout.Width(previewSize), GUILayout.Height(previewSize))){
                                            viewIndex = i + 1;
                                            currenPage = Pages.Edit;
                                        }
                                    }
                                }
                            }
                            else{
                                if (FirstCategoryFork(ItemDataBaseList.data.data[i].category) == itemCategory[z]){
                                    if (GUILayout.Button(AssetPreview.GetAssetPreview(ItemDataBaseList.data.data[i].prefab), GUILayout.Width(previewSize), GUILayout.Height(previewSize))){
                                        viewIndex = i + 1;
                                        currenPage = Pages.Edit;
                                    }
                                }
                            }
                        }
                        GUILayout.EndHorizontal();
                    }
                }
                else{
                    GUILayout.BeginHorizontal();
                    int a = 0;
                    for (int i = 0; i < ItemDataBaseList.data.data.Count; i++){
                        if ((i-a) * previewSize + previewSize + 31 >= position.width){
                            GUILayout.EndHorizontal();
                            GUILayout.BeginHorizontal();
                            a = i;
                        }
                        if (GUILayout.Button(AssetPreview.GetAssetPreview(ItemDataBaseList.data.data[i].prefab), GUILayout.Width(previewSize),GUILayout.Height(previewSize))){
                            viewIndex = i + 1;
                            currenPage = Pages.Edit;
                        }
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndScrollView();
                GUILayout.FlexibleSpace();

                GUILayout.BeginHorizontal();
                previewSize = EditorGUILayout.Slider(previewSize, 50 , 150, GUILayout.Width(position.width / 2.5f));
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Categorize", GUILayout.Width(position.width/3 + 5))){
                    categorize = !categorize;
                }
                GUILayout.EndHorizontal();

                LowerButtons();                
                break;

            case Pages.Non:

                break;
        }

        if (GUI.changed && ItemDataBaseList != null){
            EditorUtility.SetDirty(ItemDataBaseList);
            XMLManager.SaveToXMl(Application.dataPath + "/" + ItemDataBaseList.name + "/" + "ItemDataBaseXML.xml", ItemDataBaseList.data); 
        }
    }

    void CreateNewItemList(){
        viewIndex = 1;
        ItemDataBaseList = CreateItemDataBase.Create(nameItemList);
        if (ItemDataBaseList){
            ItemDataBaseList.data.data = new List<Item>();
            string relPath = AssetDatabase.GetAssetPath(ItemDataBaseList);
            EditorPrefs.SetString("ObjectPath", relPath);
            currenPage = Pages.Edit;
            FillCategoryMenu();
        }
    }

    void OpenItemList(){
        string absPath = EditorUtility.OpenFilePanel("Select Inventory Item List", "", "");
        if (absPath.StartsWith(Application.dataPath)){
            string relPath = absPath.Substring(Application.dataPath.Length - "Assets".Length);
            ItemDataBaseList = AssetDatabase.LoadAssetAtPath(relPath, typeof(ItemDataBase)) as ItemDataBase;
            if (ItemDataBaseList.data.data == null) { 
                ItemDataBaseList.data.data = new List<Item>();
            }
            if (ItemDataBaseList){
                EditorPrefs.SetString("ObjectPath", relPath);
            }
            FillCategoryMenu();
        }
    }

    void AddItem(){
        Item newItem = new Item();
        newItem.name = "New Item";
        ItemDataBaseList.data.data.Add(newItem);
        viewIndex = ItemDataBaseList.data.data.Count;
    }

    void DeleteItem(int index){
        ItemDataBaseList.data.data.RemoveAt(index);
        viewIndex = viewIndex > 0 ? viewIndex - 1 : viewIndex = 1;
    }

    void AddMenuItem(GenericMenu menu, bool standaardItem, string menuPath/*, string category*/){
        //menu.AddItem(new GUIContent(menuPath), ItemDataBaseList.data.data[viewIndex - 1].description.Equals(category), OnColorSelected, category);

        if (menu == itemCategoryTagMenu){
            if (standaardItem){
                if (LastCategoryFork(menuPath) == "Add +"){
                    menu.AddItem(new GUIContent(menuPath), false, OnMenuItemAdd, menuPath);
                }
                if (LastCategoryFork(menuPath) == "Del -"){
                    menu.AddItem(new GUIContent(menuPath), false, OnMenuItemDel, menuPath);
                }
            }
            else{
                menu.AddItem(new GUIContent(menuPath), false, OnMenuItemPress, menuPath);
                if (!itemCategory.Contains(FirstCategoryFork(menuPath))){
                    itemCategory.Add(FirstCategoryFork(menuPath));
                }
                if (!ItemDataBaseList.data.tagMenu.Contains(menuPath)){
                    ItemDataBaseList.data.tagMenu.Add(menuPath);
                }
            }
        }
        if (menu == sceneItemCategoryTagMenu){
            menu.AddItem(new GUIContent(menuPath), false, OnSceneMenuItemPress, menuPath);
        }     
    }

    void OnMenuItemPress(object category){

        if (ItemDataBaseList.data.data[viewIndex - 1].category == "-") {
            ItemDataBaseList.data.data[viewIndex - 1].category = category.ToString() + "-";
            ItemDataBaseList.data.data[viewIndex - 1].amoutOfTags++;    
        }
        else {
            for (int i = 0; i < ItemDataBaseList.data.data[viewIndex - 1].amoutOfTags; i++){
                if (category.ToString() == SepparateCategories(ItemDataBaseList.data.data[viewIndex - 1].category, i)){
                    break;
                }
                else if (i == ItemDataBaseList.data.data[viewIndex - 1].amoutOfTags - 1){
                    if (category.ToString() == SepparateCategories(ItemDataBaseList.data.data[viewIndex - 1].category, i)){
                        break;
                    }
                    ItemDataBaseList.data.data[viewIndex - 1].category += category.ToString() + "-";
                    ItemDataBaseList.data.data[viewIndex - 1].amoutOfTags++;
                }
            }
        }
    }

    void OnMenuItemAdd(object category){
        addMenuItem = !addMenuItem;
    }

    void OnMenuItemDel(object category){
        delMenuItem = !delMenuItem;
    }

    void OnSceneMenuItemPress(object category){
        currentCategory = category.ToString();
    }

    void AddMenuItemCategoryMenu(){   
        if (nameMenuItem != "" && nameMenuItem != "/ for submenu" && nameMenuItem[0].ToString() != "/" && nameMenuItem[nameMenuItem.Length-1].ToString() != "/"){
            AddMenuItem(itemCategoryTagMenu, false, nameMenuItem);
            AddMenuItem(sceneItemCategoryTagMenu, false, nameMenuItem);
        }
        nameMenuItem = "/ for submenu";
        GUI.FocusControl(null);
    }

    void DelMenuItemCategoryMenu(){//-------------------------------------------todo sure? remove obj with tag u want to del/ ok objecten die die tag hebben moeten die tag deleten.
        if (ItemDataBaseList.data.tagMenu.Contains(nameMenuItem)){
            ItemDataBaseList.data.tagMenu.Remove(nameMenuItem);
             
            FillCategoryMenu();
        }
        nameMenuItem = "/ for submenu";
        GUI.FocusControl(null);
    }

    void FillCategoryMenu(){
        currentCategory = "Category";

        itemCategory.Clear();
        itemCategory.Add("-");

        sceneItemCategoryTagMenu = new GenericMenu();
        AddMenuItem(sceneItemCategoryTagMenu,true, "All");
        AddMenuItem(sceneItemCategoryTagMenu, true, "Not categorized");
        sceneItemCategoryTagMenu.AddSeparator("");

        itemCategoryTagMenu = new GenericMenu();
        AddMenuItem(itemCategoryTagMenu, true, "Add +");
        AddMenuItem(itemCategoryTagMenu, true, "Del -");
        itemCategoryTagMenu.AddSeparator("");

        for (int i = 0; i < ItemDataBaseList.data.tagMenu.Count; i++){
            AddMenuItem(itemCategoryTagMenu, false, ItemDataBaseList.data.tagMenu[i]);
            AddMenuItem(sceneItemCategoryTagMenu, false, ItemDataBaseList.data.tagMenu[i]);
        }
    }

    string FirstCategoryFork(string p){
        string newName = "";
        int i = 0;
        if (p.Contains("/")){
            while (p[i].ToString() != "/"){
                newName = p.Substring(0,i+1);
                i++;
            }
            return newName;
        }
        else{
            return p;
        }
    }

    string LastCategoryFork(string p){
        string name = "";
        int i = p.Length - 1;
        if (p.Contains("/")){
            while (p[i].ToString() != "/"){
                name = p[i].ToString() + name;
                i--;
            }
            return name;
        }
        return p;
    }

    string SepparateCategories(string c, int pos){
        string name = "";
        int i = 0;
        if (c.Contains("-") && pos == 0){
            while (c[i].ToString() != "-"){
                name = name + c[i].ToString();
                i++;
            }
            return name;
        }
        else{
            while (pos > 0) {
                name = "";
                while (pos > 0 && c[i].ToString() != "-"){
                    i++;
                }
                while (c[i+1].ToString() != "-" && pos > 0){
                    name = name + c[i+1].ToString();
                    i++;
                }
                pos--;
            }
            return name;
        }
    }

    string RemoveCatogoryFromString(string c, int pos){
        string newCat = "";
        int i = 0;
        int startRemoveIndex = 0;
        int endRemoveIndex = 0;
        if (pos == 0){
            while (c[i].ToString() != "-"){
                endRemoveIndex = i + 2;
                i++;
            }
            newCat = c.Remove(startRemoveIndex, endRemoveIndex);
            return newCat;
        }
        else{
            int s = 0;
            while (pos > 0){
                while (c[s].ToString() != "-"){
                    startRemoveIndex = s + 1;
                    s++;
                }
                s++;
                while (pos == 1 && c[s + i].ToString() != "-"){
                    endRemoveIndex = i + 2;
                    i++;
                }
                pos--;
            }
        }
        newCat = c.Remove(startRemoveIndex, endRemoveIndex);
        return newCat;
    }

    void LowerButtons(){
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Item", GUILayout.ExpandWidth(true))){
            AddItem();
        }
        if (GUILayout.Button("Overview", GUILayout.ExpandWidth(true))){
            if (currenPage == Pages.Overview){
                currenPage = Pages.Edit;
            }
            else {
                currenPage = Pages.Overview;
            }
        }
        if (GUILayout.Button("Delete Item", GUILayout.ExpandWidth(true))){
            DeleteItem(viewIndex - 1);
        }
        GUILayout.EndHorizontal();
    }

    private void RenderToolbar(SceneView sceneView){// zorgt voor de sceneview elementen 
        Rect r = new Rect() { position = new Vector2(rectToolbarOffsetLeft, -1), width = sceneView.position.width - rectToolbarOffsetRight - 274, height = 18 };
        GUI.Window(14041190, r, RenderToolBarButton, "", GUIStyle.none);
        if (extension){
            Rect rr = new Rect() { position = new Vector2(0,17), width = sceneView.position.width, height = 18 };
            GUI.Window(34568093, rr, RenderToolBarExtensions, "", EditorStyles.toolbar);
        }
        if (showItemsInScene && currentCategory != "Category"){
            LabelGameObjects();
        }
    }

    private void RenderToolBarButton(int id){ // knopje op de toolbar van sceneview
        Rect r = new Rect() { position = Vector2.zero, height = 18, width = 40 };
        extension = GUI.Toggle(r, extension, "Tool", EditorStyles.toolbarButton);
    }

    private void RenderToolBarExtensions(int id){ // toolbar extension
        if (GUI.Button(new Rect(5, 0, 120, 18), currentCategory, EditorStyles.toolbarDropDown)){
            sceneItemCategoryTagMenu.ShowAsContext();
        }

        Rect r = new Rect(257, 0, 40, 18);
        showItemsInScene = GUI.Toggle(r, showItemsInScene, "Show", EditorStyles.toolbarButton);
    }

    void LabelGameObjects(){
        if (currentCategory == "All"){
            List<GameObject> objindatabase = new List<GameObject>();

            for (int i = 0; i < ItemDataBaseList.data.data.Count; i++){
                if (ItemDataBaseList.data.data[i].prefab != null){
                    objindatabase.Add(ItemDataBaseList.data.data[i].prefab);
                }
            }
            foreach (GameObject obj in GameObject.FindObjectsOfType<GameObject>()){
                for (int i = 0; i < objindatabase.Count; i++){
                    if (RemoveCloneTag(obj.gameObject.name) == objindatabase[i].gameObject.name){
                        Handles.Label(obj.transform.position, obj.name, EditorStyles.helpBox);

                    }
                }
            }
        }
        else if (currentCategory == "Not categorized" || currentCategory == ""){
            List<GameObject> objindatabase = new List<GameObject>();

            for (int i = 0; i < ItemDataBaseList.data.data.Count; i++){
                if ("-" == ItemDataBaseList.data.data[i].category && ItemDataBaseList.data.data[i].prefab != null){
                    objindatabase.Add(ItemDataBaseList.data.data[i].prefab);
                }
            }

            foreach (GameObject obj in GameObject.FindObjectsOfType<GameObject>()){
                for (int i = 0; i < objindatabase.Count; i++){
                    if (RemoveCloneTag(obj.gameObject.name) == objindatabase[i].gameObject.name){
                        Handles.Label(obj.transform.position, obj.name, EditorStyles.helpBox);
                    }
                }
            }
        }
        else{
            List<GameObject> objindatabase = new List<GameObject>();

            for (int i = 0; i < ItemDataBaseList.data.data.Count; i++){
                if (ItemDataBaseList.data.data[i].amoutOfTags == 1){
                    if (currentCategory == SepparateCategories(ItemDataBaseList.data.data[i].category, 0) && ItemDataBaseList.data.data[i].prefab != null){
                        objindatabase.Add(ItemDataBaseList.data.data[i].prefab);
                    }
                }
                else { 
                    for (int y = 0; y < ItemDataBaseList.data.data[i].amoutOfTags; y++){
                        if (currentCategory == SepparateCategories(ItemDataBaseList.data.data[i].category, y) && ItemDataBaseList.data.data[i].prefab != null){
                            objindatabase.Add(ItemDataBaseList.data.data[i].prefab);
                        }
                    }
                } 
            }
            foreach (GameObject obj in GameObject.FindObjectsOfType<GameObject>()){
                for (int i = 0; i < objindatabase.Count; i++){
                    if (RemoveCloneTag(obj.gameObject.name) == objindatabase[i].gameObject.name){
                        Handles.Label(obj.transform.position, obj.name, EditorStyles.helpBox);
                    }
                }
            }
        }
    }

    List<Vector3> GetItemPos(){
        List<Vector3> returnArray = new List<Vector3>();

        if (ItemDataBaseList.data.data[viewIndex - 1].prefab != null){
            foreach (GameObject obj in GameObject.FindObjectsOfType<GameObject>()){            
                if (obj.name == ItemDataBaseList.data.data[viewIndex-1].prefab.name){
                    returnArray.Add(obj.transform.position);
                }
            }
        }
        else {
            returnArray.Add(Vector3.zero);
        }      
        return returnArray;      
    }

    string RemoveCloneTag(string name){
        string newName = "";
        int i = name.Length - 1;
        if (name.Contains(" (") && name[name.Length-1].ToString() == ")"){
            while (name[i].ToString() != "("){
                newName = name.Substring(0,i-2);
                i--;
            }
            return newName;
        }
        else{
            return name;
        }
    }

    void DuplicatePrefabsSetInFolder(){
        for (int i = 0; i < ItemDataBaseList.data.data.Count; i++){
            if (ItemDataBaseList.data.data[i].prefab != null){               /// folder waar list in zit vullen met gebruikte prefabs.
                if (System.IO.File.Exists(AssetDatabase.GetAssetPath(ItemDataBaseList).Substring(0, (AssetDatabase.GetAssetPath(ItemDataBaseList)).Length - LastCategoryFork(AssetDatabase.GetAssetPath(ItemDataBaseList)).Length) + ItemDataBaseList.data.data[i].prefab.name + ".prefab") == false){
                    int removeFromPath = LastCategoryFork(AssetDatabase.GetAssetPath(ItemDataBaseList)).Length;
                    string toPath = AssetDatabase.GetAssetPath(ItemDataBaseList).Substring(0, removeFromPath + 2) + ItemDataBaseList.data.data[i].prefab.name + ".prefab";
                    AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(ItemDataBaseList.data.data[i].prefab), toPath);
                    AssetDatabase.Refresh();
                }
            }
        }
                   
    }

    void RemoveOldPrefabFromFolder(){
        if (System.IO.File.Exists(AssetDatabase.GetAssetPath(ItemDataBaseList).Substring(0, (AssetDatabase.GetAssetPath(ItemDataBaseList)).Length - LastCategoryFork(AssetDatabase.GetAssetPath(ItemDataBaseList)).Length) + currentPrefab + ".prefab")){
            int removeFromPath = LastCategoryFork(AssetDatabase.GetAssetPath(ItemDataBaseList)).Length;
            FileUtil.DeleteFileOrDirectory(AssetDatabase.GetAssetPath(ItemDataBaseList).Substring(0, removeFromPath + 2) + currentPrefab + ".prefab");
            AssetDatabase.Refresh();
        }
        
    }

    void LoadInPrefabs(){
        for (int i = 0; i < ItemDataBaseList.data.data.Count; i++){
            if (ItemDataBaseList.data.data[i].prefabName != null && ItemDataBaseList.data.data[i].prefabName != ""){
                ItemDataBaseList.data.data[i].prefab = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/" + ItemDataBaseList.name + "/" + ItemDataBaseList.data.data[i].prefabName + ".prefab", typeof(GameObject));
            }
        }
    }

    void LoadSarahsXML(){
        //Debug.Log(Application.dataPath + "/ToolFinal" + "/" + "ComboData.xml");
        //XMLManager.LoadFromXML(Application.dataPath + "/ToolFinal" + "/" + "ComboData.xml");
        string[] XMLLine = System.IO.File.ReadAllLines(Application.dataPath + "/ToolFinal" + "/" + "ComboData.xml");
        bool inCombo = false;
        string combo = "";       
        int indexOfData = 0;



        for (int i = 0; i < XMLLine.Length; i++){
            if (inCombo == false && XMLLine[i].Contains("<Combo>")){
                inCombo = true;
                continue;
            }

            if (inCombo && XMLLine[i].Contains("</Combo>")){
                inCombo = false;
                if (ItemDataBaseList.data.data.Count <= indexOfData){
                    break;
                }
                ItemDataBaseList.data.data[indexOfData].description = combo;
                combo = "";
                indexOfData++;
                continue;
            }

            if (inCombo){
                combo += RemoveXMLTags(XMLLine[i]);
            }
        }

    }

    string RemoveXMLTags(string name){
        string newName = "";
        int i = name.Length - 1;
        if (name.Contains("<")){
            while (name[i].ToString() != "<"){
                newName = name.Substring(0, i - 1);
                i--;
            }
        }
        if (newName.Contains(">")){
            while (newName[0].ToString() != ">"){
                newName = newName.Substring(1, newName.Length - 1);
            }
            newName = newName.Substring(1, newName.Length - 1);
            return newName;
        }
        return newName;       
    }

}
