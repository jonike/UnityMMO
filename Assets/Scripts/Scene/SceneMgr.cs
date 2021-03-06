using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using XLua;

namespace UnityMMO
{
    
[Hotfix]
[LuaCallCSharp]
public class SceneMgr : MonoBehaviour
{
	public static SceneMgr Instance;
    public Transform container;
	EntityManager entityManager;
    public EntityArchetype RoleArchetype;
    public EntityArchetype MonsterArchetype;
    public EntityArchetype NPCArchetype;
    Dictionary<long, Entity> entityDic;
    Entity mainRole;
    public SceneDetectorBase detector;
    private SceneObjectLoadController m_Controller;
    const string SceneInfoPath = "Assets/AssetBundleRes/scene/";

    public EntityManager EntityManager { get => entityManager; set => entityManager = value; }

    public void Awake()
	{
		Instance = this; // worst singleton ever but it works
		EntityManager = World.Active.GetExistingManager<EntityManager>();
        entityDic = new Dictionary<long, Entity>();
	}

    void Update()
    {
        if (detector != null && m_Controller != null)
            m_Controller.RefreshDetector(detector);
    }

	public void InitArcheType()
	{
		RoleArchetype = EntityManager.CreateArchetype(
                typeof(Position),typeof(TargetPosition),
                typeof(MoveSpeed));
	}

	public void OnDestroy()
	{
		Instance = null;
	}

    private static string Repalce(string str)
    {
        str = System.Text.RegularExpressions.Regex.Replace(str, @"<", "lt;");
        str = System.Text.RegularExpressions.Regex.Replace(str, @">", "gt;");
        // str = System.Text.RegularExpressions.Regex.Replace(str, @"\\", "quot;");
        str = System.Text.RegularExpressions.Regex.Replace(str, @"\r", "");
        str = System.Text.RegularExpressions.Regex.Replace(str, @"\n", "");
        str = System.Text.RegularExpressions.Regex.Replace(str, @"\/", "/");
        return str;
    }

    public void LoadScene(int scene_id, float pos_x=0.0f, float pos_y=0.0f, float pos_z=0.0f)
    {
        Debug.Log("LoadScene scene_id "+(scene_id).ToString());
        //load scene info from json file(which export from SceneInfoExporter.cs)
        XLuaFramework.ResourceManager.GetInstance().LoadAsset<TextAsset>(SceneInfoPath+"scene_"+scene_id.ToString() +"/scene_info.json", delegate(UnityEngine.Object[] objs) {
            TextAsset txt = objs[0] as TextAsset;
            string scene_json = txt.text;
            scene_json = Repalce(scene_json);
            SceneInfo scene_info = JsonUtility.FromJson<SceneInfo>(scene_json);
            ApplyLightInfo(scene_info);
            
            m_Controller = gameObject.GetComponent<SceneObjectLoadController>();
            if (m_Controller == null)
                m_Controller = gameObject.AddComponent<SceneObjectLoadController>();

            int max_create_num = 19;
            int min_create_num = 0;
            m_Controller.Init(scene_info.Bounds.center, scene_info.Bounds.size, true, max_create_num, min_create_num, SceneSeparateTreeType.QuadTree);

            Debug.Log("scene_info.ObjectInfoList.Count : "+scene_info.ObjectInfoList.Count.ToString());
            for (int i = 0; i < scene_info.ObjectInfoList.Count; i++)
            {
                m_Controller.AddSceneBlockObject(scene_info.ObjectInfoList[i]);
            }
        });
    }
    LightmapData[] lightmaps = null;

    private void ApplyLightInfo(SceneInfo scene_info)
    {
        LightmapSettings.lightmapsMode = scene_info.LightmapMode;
        int l1 = (scene_info.LightColorResPath == null) ? 0 : scene_info.LightColorResPath.Count;
        int l2 = (scene_info.LightDirResPath == null) ? 0 : scene_info.LightDirResPath.Count;
        int l = (l1 < l2) ? l2 : l1;
        // Debug.Log("ApplyLightInfo : "+ l.ToString());
        if (l > 0)
        {
            lightmaps = new LightmapData[l];
            for (int i = 0; i < l; i++)
            {
                if (i < l1)
                {
                    int temp_i = i;
                    XLuaFramework.ResourceManager.GetInstance().LoadAsset<Texture2D>(scene_info.LightColorResPath[i], delegate(UnityEngine.Object[] objs) {
                        // Debug.Log("load lightmap color texture : "+scene_info.LightColorResPath[i].ToString());
                        // Debug.Log("i : "+temp_i.ToString()+" objs:"+(objs!=null).ToString());
                        lightmaps[temp_i] = new LightmapData();
                        if (objs != null && objs.Length > 0)
                            lightmaps[temp_i].lightmapColor = objs[0] as Texture2D;
                        if (temp_i == l-1)
                            LightmapSettings.lightmaps = lightmaps;
                    });
                }
                if (i < l2)
                {
                    int temp_i = i;
                    XLuaFramework.ResourceManager.GetInstance().LoadAsset<Texture2D>(scene_info.LightDirResPath[i], delegate(UnityEngine.Object[] objs) {
                        // Debug.Log("load lightmap dir texture : "+scene_info.LightDirResPath[i].ToString());
                        lightmaps[temp_i] = new LightmapData();
                        if (objs != null && objs.Length > 0)
                            lightmaps[temp_i].lightmapDir = objs[0] as Texture2D;
                        if (temp_i == l-1)
                            LightmapSettings.lightmaps = lightmaps;
                    });
                }
            }
 
        }
    }

    public Entity AddMainRole(long uid)
	{
		Entity role = AddRole(uid);
        EntityManager.AddComponent(role, ComponentType.Create<PlayerInput>());
        EntityManager.AddComponent(role, ComponentType.Create<SynchPosFlag>());
        entityDic.Add(uid, role);
        mainRole = role;
        return role;
	}

    public Entity AddRole(long uid)
	{
		Entity role = EntityManager.CreateEntity(RoleArchetype);
        EntityManager.SetComponentData(role, new Position {Value = new int3(0, 0, 0)});
        EntityManager.SetComponentData(role, new MoveSpeed {speed = 12});
        EntityManager.SetComponentData(role, new TargetPosition {Value = new int3(0, 0, 0)});
        EntityManager.AddSharedComponentData(role, GetLookFromPrototype("Prototype/MainRoleRenderPrototype"));
        entityDic.Add(uid, role);
        return role;
	}

    public Entity AddSceneObject(long uid, SceneObjectType type)
    {
        if (type == SceneObjectType.Role)
            return AddRole(uid);
        return Entity.Null;
    }

    public void RemoveSceneObject(long uid)
    {
        Entity entity = GetSceneObject(uid);
        if (entity!=Entity.Null)
            entityManager.DestroyEntity(entity);
    }

    public Entity GetSceneObject(long uid)
    {
        Debug.Log("GetSceneObject uid"+uid.ToString()+" ContainsKey:"+entityDic.ContainsKey(uid).ToString());
        if (entityDic.ContainsKey(uid))
            return entityDic[uid];
        return Entity.Null;
    }

	private MeshInstanceRenderer GetLookFromPrototype(string protoName)
    {
        var proto = GameObject.Find(protoName);
        var result = proto.GetComponent<MeshInstanceRendererComponent>().Value;
        // Object.Destroy(proto);
        return result;
    }
}

}