/*
 * Created by jiadong chen
 * http://www.chenjd.me
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class AnimMapBakerWindow : EditorWindow {

    private enum SaveStrategy
    {
        AnimMap,//only anim map
        Mat,//with shader
        Prefab//prefab with mat
    }

    #region 字段

    public static GameObject targetGo;
    private static AnimMapBaker baker;
    private static string path = "DefaultPath";
    private static SaveStrategy stratege = SaveStrategy.Prefab;
    private static Shader animMapShader;

    #endregion


    #region  方法

    [MenuItem("GpuInstancingAnim/AnimMapBaker")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(AnimMapBakerWindow));
        baker = new AnimMapBaker();
        animMapShader = Shader.Find("Custom/CharactorShader");
    }

    public static GameObject oldGameObject = null;
   
    GameObject animationFBX;
    GameObject rootBone;
    void OnGUI()
    {
        GUILayout.Label("Target Prefab");
        GameObject oldObj = targetGo;
        targetGo = (GameObject)EditorGUILayout.ObjectField(targetGo, typeof(GameObject), true);
        GUILayout.Label("Target Animation FBX");
        animationFBX = (GameObject)EditorGUILayout.ObjectField(animationFBX, typeof(GameObject), true);
        if(targetGo != oldGameObject)
        {
            oldGameObject = targetGo;
        }

        GUILayout.Label("RootBone GameObject");
        rootBone = (GameObject)EditorGUILayout.ObjectField(rootBone, typeof(GameObject), true);

        Object target = PrefabUtility.GetPrefabParent(targetGo);
        path = AssetDatabase.GetAssetPath(target);
        if(path == "")
        {
            path = AssetDatabase.GetAssetPath(targetGo);
        }
        int index = path.LastIndexOf("/");
       
        if(index >= 0)
        {
            path = path.Substring(7, index - 7);
        }
        
        EditorGUILayout.LabelField(string.Format("output path:{0}", path));
        path = EditorGUILayout.TextField(path);
       
        if (GUILayout.Button("Bake"))
        {
            if(targetGo == null)
            {
                EditorUtility.DisplayDialog("err", "targetGo is null！", "OK");
                return;
            }

            baker = new AnimMapBaker();

            baker.SetAnimData(targetGo, animationFBX, path);
            Dictionary<string, string> dic = new Dictionary<string, string>();
            Mesh tarMesh = null;
            List<BakedData> list = baker.Bake(targetGo, animationFBX, rootBone, path, dic, ref tarMesh);

            if(list != null)
            {
                for(int i = 0; i < list.Count; i++)
                {
                    BakedData data = list[i];
                    Save(ref data, i, tarMesh);
                }
            }

           
        }
    }


    private void Save(ref BakedData data, int i, Mesh mesh)
    {
        if(i == 0)
        {
            SaveAsPrefab(ref data, i, mesh);
        }
        else
        {
            SaveAsAsset(ref data, i);
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private Texture2D SaveAsAsset(ref BakedData data, int i)
    {
        string folderPath = CreateFolder();
        Texture2D animMap = new Texture2D(data.animMapWidth, data.animMapHeight, AnimMapBaker.format, false);
        animMap.filterMode = FilterMode.Point;
        animMap.LoadRawTextureData(data.rawAnimMap);
        AssetDatabase.CreateAsset(animMap, Path.Combine(folderPath, data.name + ".asset"));
        return animMap;
    }


   
    private Material SaveAsMat(ref BakedData data, int i)
    {
        if(animMapShader == null)
        {
            animMapShader = Shader.Find("Custom/CharactorShader");
        }

        if(targetGo == null || !targetGo.GetComponentInChildren<SkinnedMeshRenderer>())
        {
            EditorUtility.DisplayDialog("err", "SkinnedMeshRender is null!!", "OK");
            return null;
        }

        SkinnedMeshRenderer smr = targetGo.GetComponentInChildren<SkinnedMeshRenderer>();
        Material mat = new Material(animMapShader);
        Texture2D animMap = SaveAsAsset(ref data, i);
        mat.SetTexture("_MainTex", smr.sharedMaterial.mainTexture);
        mat.SetTexture("_AnimMap", animMap);
        mat.SetFloat("_AnimAll", data.animLen);
        string folderPath = CreateFolder();
        string matName = data.name;
        
        //if (data.name.LastIndexOf("_") >= 0)
        //{
        //    matName = data.name.Substring(0, data.name.LastIndexOf("_"));
        //}
        matName += "matbaked";
        AssetDatabase.CreateAsset(mat, Path.Combine(folderPath, matName + ".mat"));

        return mat;
    }

    private void SaveAsPrefab(ref BakedData data, int i, Mesh mesh)
    {
        Material mat = SaveAsMat(ref data, i);

        if(mat == null)
        {
            EditorUtility.DisplayDialog("err", "mat is null!!", "OK");
            return;
        }

        GameObject go = new GameObject();
        go.AddComponent<MeshRenderer>().sharedMaterial = mat;
        go.AddComponent<MeshFilter>().sharedMesh = mesh;
        mat.enableInstancing = true;
        string folderPath = CreateFolder();
        string prefabName = data.name;
        
        //if (data.name.LastIndexOf("_") >= 0)
        //{
        //    prefabName = data.name.Substring(0, data.name.LastIndexOf("_"));
        //}
        
        prefabName += "prefab";
        PrefabUtility.CreatePrefab(Path.Combine(folderPath, prefabName + ".prefab").Replace("\\", "/"), go);
        GameObject.DestroyImmediate(go);
    }

    private string CreateFolder()
    {
        string folderPath = "Assets/" + path;
        return folderPath;
    }

    #endregion


}
