/*
 * Created by jiadong chen
 * http://www.chenjd.me
 * 
 * 用来烘焙动作贴图。烘焙对象使用animation组件，并且在导入时设置Rig为Legacy
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;

/// <summary>
/// 保存需要烘焙的动画的相关数据
/// </summary>
public class AnimData
{
    #region 字段

    public List<AnimationState> animClips;
    public string name;

    public Animation animation;
    public SkinnedMeshRenderer skin;

    #endregion

    public AnimData(Animation anim, SkinnedMeshRenderer smr, string goName)
    {
        animClips = new List<AnimationState>(anim.Cast<AnimationState>());
        animation = anim;
        skin = smr;
        name = goName;
    }

    #region 方法

    public void AnimationPlay(string animName)
    {
        this.animation.Play(animName);
    }

    public void SampleAnimAndBakeMesh(ref Mesh m)
    {
        this.SampleAnim();
        this.BakeMesh(ref m);
        
    }

    private void SampleAnim()
    {
        if (this.animation == null)
        {
            Debug.LogError("animation is null!!");
            return;
        }

        this.animation.Sample();
    }

    private void BakeMesh(ref Mesh m)
    {
        if (this.skin == null)
        {
            Debug.LogError("skin is null!!");
            return;
        }

        this.skin.BakeMesh(m);
    }


    #endregion

}

/// <summary>
/// 烘焙后的数据
/// </summary>
public struct BakedData
{
    #region 字段

    public string name;
    public float animLen;
    public byte[] rawAnimMap;
    public int animMapWidth;
    public int animMapHeight;

    #endregion

    public BakedData(string name, float animLen, Texture2D animMap)
    {
        this.name = name;
        this.animLen = animLen;
        this.animMapHeight = animMap.height;
        this.animMapWidth = animMap.width;
        this.rawAnimMap = animMap.GetRawTextureData();
    }
}

/// <summary>
/// 烘焙器
/// </summary>
public class AnimMapBaker{

    #region 字段

    private AnimData animData;
    private List<Vector3> vertices = new List<Vector3>();
    private Mesh bakedMesh;

    private List<BakedData> bakedDataList = new List<BakedData>();
    public static TextureFormat format = TextureFormat.RGBAHalf;
    #endregion

    #region 方法

    public void SetAnimData(GameObject go, GameObject fbx, string path)
    {
        if(go == null)
        {
            Debug.LogError("go is null!!");
            return;
        }

        Animation anim = go.GetComponent<Animation>();
        if(anim == null || anim.GetClipCount() == 0)
        {
            if(anim == null)
            {
                anim = go.AddComponent<Animation>();
            }
            Object target = PrefabUtility.GetPrefabParent(fbx);
            string fbxpath = AssetDatabase.GetAssetPath(target);
            if (target == null)
            {
                fbxpath = AssetDatabase.GetAssetPath(fbx);
            }
            else
            {
                
            }
           
            Debug.Log(fbxpath);
            ModelImporter modelImporter = (ModelImporter)AssetImporter.GetAtPath(fbxpath);
            if(modelImporter.animationType != ModelImporterAnimationType.Legacy)
            {
                modelImporter.animationType = ModelImporterAnimationType.Legacy;
                modelImporter.SaveAndReimport();
            }
            Object[] arr = AssetDatabase.LoadAllAssetsAtPath(fbxpath);
            for(int i = 0; i < modelImporter.clipAnimations.Length; i++)
            {
                ModelImporterClipAnimation clip = modelImporter.clipAnimations[i];
                for(int j = 0; j < arr.Length; j++)
                {
                    if(arr[j] as AnimationClip)
                    {
                        if(arr[j].name == clip.name)
                        {
                            anim.AddClip(arr[j] as AnimationClip, arr[j].name);
                            break;
                        }
                    }
                }
                
            }
        }
        SkinnedMeshRenderer smr = go.GetComponentInChildren<SkinnedMeshRenderer>();

        if(anim == null || smr == null)
        {
            Debug.LogError("anim or smr is null!!");
            return;
        }
        this.bakedMesh = new Mesh();
        this.animData = new AnimData(anim, smr, go.name);
    }
    private class BoneWeightSortData : System.IComparable<BoneWeightSortData>
    {
        public int index = 0;

        public float weight = 0;

        public int CompareTo(BoneWeightSortData b)
        {
            return weight > b.weight ? -1 : 1;
        }
    }

    private GPUSkinningBone GetBoneByTransform(Transform transform, GPUSkinningBone[] bones)
    {
        int numBones = bones.Length;
        for (int i = 0; i < numBones; ++i)
        {
            if (bones[i].transform == transform)
            {
                return bones[i];
            }
        }
        return null;
    }

    private int GetBoneIndex(GPUSkinningBone bone, GPUSkinningBone[] bones)
    {
        return System.Array.IndexOf(bones, bone);
    }
    public List<BakedData> Bake(GameObject go, GameObject fbx, GameObject rootBone, string path, Dictionary<string, string> dic, ref Mesh tarMesh)
    {
        if (this.animData == null)
        {
            Debug.LogError("bake data is null!!");
            return this.bakedDataList;
        }


        List<GPUSkinningBone> bones_result = new List<GPUSkinningBone>();
        SkinnedMeshRenderer skin = animData.skin;
        CollectBones(bones_result, skin.bones, skin.sharedMesh.bindposes, null, rootBone.transform, 0);
        GPUSkinningBone[] bones = bones_result.ToArray();
        Mesh mesh = new Mesh();
        mesh.name = skin.sharedMesh.name;
        mesh.vertices = skin.sharedMesh.vertices;
        mesh.normals = skin.sharedMesh.normals;
        mesh.tangents = skin.sharedMesh.tangents;
        mesh.uv = skin.sharedMesh.uv;
        BoneWeight[] boneWeights = skin.sharedMesh.boneWeights;
        int numVertices = mesh.vertices.Length;
        Vector4[] uv2 = new Vector4[numVertices];
        Vector4[] uv3 = new Vector4[numVertices];
        Transform[] smrBones = skin.bones;
        
        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            BoneWeight weight = boneWeights[i];
            BoneWeightSortData[] weights = new BoneWeightSortData[4];
            weights[0] = new BoneWeightSortData() { index = weight.boneIndex0, weight = weight.weight0 };
            weights[1] = new BoneWeightSortData() { index = weight.boneIndex1, weight = weight.weight1 };
            weights[2] = new BoneWeightSortData() { index = weight.boneIndex2, weight = weight.weight2 };
            weights[3] = new BoneWeightSortData() { index = weight.boneIndex3, weight = weight.weight3 };
            System.Array.Sort(weights);
            //Debug.Log(weights[0].index + ":" + weights[0].weight + ":" + weights[1].index + ":" + weights[1].weight + ":" + weights[2].index + ":" + weights[2].weight + ":" + weights[3].index + ":" + weights[3].weight);
            GPUSkinningBone bone0 = GetBoneByTransform(smrBones[weights[0].index], bones);
            GPUSkinningBone bone1 = GetBoneByTransform(smrBones[weights[1].index], bones);
            GPUSkinningBone bone2 = GetBoneByTransform(smrBones[weights[2].index], bones);
            GPUSkinningBone bone3 = GetBoneByTransform(smrBones[weights[3].index], bones);

            Vector4 skinData_01 = new Vector4();
            skinData_01.x = GetBoneIndex(bone0, bones);
            skinData_01.y = weights[0].weight;
            skinData_01.z = GetBoneIndex(bone1, bones);
            skinData_01.w = weights[1].weight;
            uv2[i] = skinData_01;
        }

        mesh.SetUVs(1, new List<Vector4>(uv2));
        mesh.triangles = skin.sharedMesh.triangles;

        AssetDatabase.CreateAsset(mesh, "Assets/" +Path.Combine(path, mesh.name + "mesh.asset"));

        tarMesh = mesh;
        BakeAllAnimClip(this.animData.animClips, path, go,  30f, rootBone, bones);

        Object target = PrefabUtility.GetPrefabParent(fbx);
        string fbxName = AssetDatabase.GetAssetPath(target);
        if (target == null)
        {
            fbxName = AssetDatabase.GetAssetPath(fbx);
        }
        else
        {
            fbxName = "Assets/" + path + "/" + fbx.name + ".fbx";
        }
        Debug.Log(fbxName);
        ModelImporter modelImporter = (ModelImporter)AssetImporter.GetAtPath(fbxName);
        
        GenAnimationDataFile(modelImporter.clipAnimations, this.animData.animClips, 30f, go, path, dic);

        return this.bakedDataList;
    }

    

    void GenAnimationDataFile(ModelImporterClipAnimation[] clips, List<AnimationState> curAnims, float frameRate, GameObject go, string path, Dictionary<string, string> dic)
    {
        string luaPath = "Assets/GpuInstancingAnimation/Resources/AnimationData";
        luaPath = Path.Combine(luaPath, go.name + "prefabAnimationData.txt");
        if(File.Exists(luaPath))
        {
            File.Delete(luaPath);
        }

        float s = 1;

        float startTime = 0f;
        
        string str = "";

        for (int i = 0; i < curAnims.Count; i++)
        {
            ModelImporterClipAnimation curClip = null;
            for (int j = 0; j < clips.Length; j++)
            {
                if(curAnims[i].name == clips[j].name)
                {
                    curClip = clips[j];
                    break;
                }
            }
            string isLoop = "False";
            if(curClip.wrapMode == WrapMode.Loop)
            {
                isLoop = "True";
            }
            float time = (Mathf.CeilToInt(curAnims[i].clip.frameRate * curAnims[i].length * s) + 2) / frameRate;

            float targetEndTime = time + startTime;
            str += curClip.name + "|" + (startTime + 0.5 / frameRate) + "|" + (targetEndTime - 0.5f / frameRate) + "|" + isLoop;

            str += "\n";

            startTime += time;
        }

        string end = "\n";
        str += end;
        Debug.Log(str);
        File.WriteAllText(luaPath, str);
    }

    int getZhengFangSize(int mianji)
    {
        int pow = 1;
        for(int i = 1; ;i++)
        {
            pow = pow * 2;
            if(pow >= mianji)
            {
                return pow;
            }
        }
    }
    float times;
    Vector3[] vertArr;
   
    private void CollectBones(List<GPUSkinningBone> bones_result, Transform[] bones_smr, Matrix4x4[] bindposes, GPUSkinningBone parentBone, Transform currentBoneTransform, int currentBoneIndex)
    {
        GPUSkinningBone currentBone = new GPUSkinningBone();
        bones_result.Add(currentBone);
       
        int indexOfSmrBones = System.Array.IndexOf(bones_smr, currentBoneTransform);
        currentBone.transform = currentBoneTransform;
        currentBone.name = currentBone.transform.gameObject.name;
        currentBone.bindpose = indexOfSmrBones == -1 ? Matrix4x4.identity : bindposes[indexOfSmrBones];
        currentBone.parentBoneIndex = parentBone == null ? -1 : bones_result.IndexOf(parentBone);

        if (parentBone != null)
        {
            parentBone.childrenBonesIndices[currentBoneIndex] = bones_result.IndexOf(currentBone);
        }

        int numChildren = currentBone.transform.childCount;
        if (numChildren > 0)
        {
            currentBone.childrenBonesIndices = new int[numChildren];
            for (int i = 0; i < numChildren; ++i)
            {
                CollectBones(bones_result, bones_smr, bindposes, currentBone, currentBone.transform.GetChild(i), i);
            }
        }
    }

    private void BakeAllAnimClip(List<AnimationState> curAnims, string path, GameObject go, float frameRate, GameObject rootBone, GPUSkinningBone[] bones)
    {
        int curClipFrame = 0;
        float sampleTime = 0;
        float perFrameTime = 0;
        float len = 0;
        float s = 1;
        for (int i = 0; i < this.animData.animClips.Count; i++)
        {
            if (!this.animData.animClips[i].clip.legacy)
            {
                Debug.LogError(string.Format("{0} is not legacy!!", this.animData.animClips[i].clip.name));
                continue;
            }
            if (this.animData.animClips[i].name.IndexOf("_all") >= 0)
            {
                continue;
            }
            
            //前后各加一帧
            curClipFrame += Mathf.CeilToInt(curAnims[i].clip.frameRate * curAnims[i].length * s) + 2;
        }

        len = curClipFrame / frameRate;
        int pow1 = Mathf.NextPowerOfTwo(curClipFrame);
        int width = Mathf.NextPowerOfTwo(bones.Length * 3);
        Texture2D animMap = new Texture2D(width, pow1, format, false);
        animMap.wrapMode = TextureWrapMode.Clamp;
        animMap.filterMode = FilterMode.Point;

        animMap.name = this.animData.name;
        int startFrame = 0;

        for (int i = 0; i < this.animData.animClips.Count; i++)
        {
            if (!this.animData.animClips[i].clip.legacy)
            {
                Debug.LogError(string.Format("{0} is not legacy!!", this.animData.animClips[i].clip.name));
                continue;
            }
            if (this.animData.animClips[i].name.IndexOf("_all") >= 0)
            {
                continue;
            }

            animData.AnimationPlay(curAnims[i].name);
            sampleTime = 0;
            int curClipFrame1 = Mathf.CeilToInt(curAnims[i].clip.frameRate * curAnims[i].length * s) + 2;
            perFrameTime = curAnims[i].length / (curClipFrame1 - 2);
            for (int k = startFrame; k < curClipFrame1 + startFrame - 2; k++)
            {
                if (sampleTime > curAnims[i].length)
                {
                    sampleTime = curAnims[i].length;
                }
                curAnims[i].time = sampleTime;

                this.animData.animation.Sample();
                for (int j = 0; j < bones.Length; j++)
                {
                    GPUSkinningBone currentBone = bones[j];
                    Matrix4x4 lastMat = currentBone.bindpose;
                    while (true)
                    {
                        if (currentBone.parentBoneIndex == -1)
                        {
                            Matrix4x4 mat = Matrix4x4.TRS(currentBone.transform.localPosition, currentBone.transform.localRotation, currentBone.transform.localScale);
                            if(rootBone.transform != go.transform)
                            {
                                mat = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, go.transform.localScale) * mat;
                            }
                            
                            lastMat = mat * lastMat;
                            break;
                        }
                        else
                        {
                            Matrix4x4 mat = Matrix4x4.TRS(currentBone.transform.localPosition, currentBone.transform.localRotation, currentBone.transform.localScale);
                            lastMat = mat * lastMat;
                            currentBone = bones[currentBone.parentBoneIndex];
                        }
                    }

                    animMap.SetPixel(j * 3, k + 1, new Color(lastMat.m00, lastMat.m01, lastMat.m02, lastMat.m03));
                    animMap.SetPixel(j * 3 + 1, k + 1, new Color(lastMat.m10, lastMat.m11, lastMat.m12, lastMat.m13));
                    animMap.SetPixel(j * 3 + 2, k + 1, new Color(lastMat.m20, lastMat.m21, lastMat.m22, lastMat.m23));

                    if (k == startFrame)
                    {
                        animMap.SetPixel(j * 3, k, new Color(lastMat.m00, lastMat.m01, lastMat.m02, lastMat.m03));
                        animMap.SetPixel(j * 3 + 1, k, new Color(lastMat.m10, lastMat.m11, lastMat.m12, lastMat.m13));
                        animMap.SetPixel(j * 3 + 2, k, new Color(lastMat.m20, lastMat.m21, lastMat.m22, lastMat.m23));
                    }
                    else if(k == curClipFrame1 + startFrame - 3)
                    {
                        animMap.SetPixel(j * 3, k + 2, new Color(lastMat.m00, lastMat.m01, lastMat.m02, lastMat.m03));
                        animMap.SetPixel(j * 3 + 1, k + 2, new Color(lastMat.m10, lastMat.m11, lastMat.m12, lastMat.m13));
                        animMap.SetPixel(j * 3 + 2, k + 2, new Color(lastMat.m20, lastMat.m21, lastMat.m22, lastMat.m23));
                    }
                   
                }
                sampleTime += perFrameTime;
            }

            startFrame += curClipFrame1;
        }

        animMap.Apply();
        this.bakedDataList.Add(new BakedData(animMap.name, pow1 / frameRate, animMap));
    }
   
    #endregion


    #region 属性


    #endregion

}
