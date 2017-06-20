
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using ICSharpCode.SharpZipLib.Zip;
namespace KK.Frame.Loader.ABS
{
    public class AssetBundleBuildPanel : EditorWindow
    {
        //[MenuItem("ABSystem/Builder Panel")]
        static void Open()
        {
            GetWindow<AssetBundleBuildPanel>("ABSystem", true);
        }
       
        [MenuItem("ABSystem/Builde AssetBundles")]
        public static void BuildAssetBundles()
        {
            AssetBundleBuildConfig config = LoadAssetAtPath<AssetBundleBuildConfig>(savePath);

            if (config == null)
                return;

#if UNITY_5
			ABBuilder builder = new AssetBundleBuilder5x(new AssetBundlePathResolver());
#else
			ABBuilder builder = new AssetBundleBuilder4x(new AssetBundlePathResolver());
#endif
            builder.SetDataWriter(config.depInfoFileFormat == AssetBundleBuildConfig.Format.Text ? new AssetBundleDataWriter() : new AssetBundleDataBinaryWriter());

            builder.Begin();

            for (int i = 0; i < config.filters.Count; i++)
            {
                AssetBundleFilter f = config.filters[i];
                if (f.valid)
                    builder.AddRootTargets(new DirectoryInfo(f.path), new string[] { f.filter });
            }

            builder.Export();
            builder.End();
        }
        [MenuItem("ABSystem/Builder Zip")]
        static void PackFile()
        {
            do
            {
                if (!Directory.Exists(Application.streamingAssetsPath))
                {
                    EditorUtility.DisplayDialog("生成压缩包", "路径不存在: \n" + Application.streamingAssetsPath + "！", "关  闭");
                    continue;
                }
                if (File.Exists(Application.streamingAssetsPath + "/AssetBundles.zip"))
                {
                    File.Delete(Application.streamingAssetsPath + "/AssetBundles.zip");
                }

                if (!Directory.Exists(Application.streamingAssetsPath + "/AssetBundles"))
                {
                    EditorUtility.DisplayDialog("生成压缩包", "路径不存在: \n" + Application.streamingAssetsPath + "/AssetBundles！", "关  闭");
                    continue;
                }
               
                    FastZip fz = new FastZip();
                    fz.CreateZip(Application.dataPath + "/AssetBundles.zip", Application.streamingAssetsPath + "/AssetBundles", true, "");
              

               

                if (!Directory.Exists(Application.streamingAssetsPath + "/AssetBundles"))
                {
                    Directory.Delete(Application.streamingAssetsPath + "/AssetBundles", true);
                }
                File.Move(Application.dataPath + "/AssetBundles.zip", Application.streamingAssetsPath + "/AssetBundles.zip"); 
                EditorUtility.DisplayDialog("生成压缩包", "完成！", "关  闭");

            } while (false);
        }
        public static T LoadAssetAtPath<T>(string path) where T:Object
		{
#if UNITY_5
			return AssetDatabase.LoadAssetAtPath<T>(path);
#else
			return (T)AssetDatabase.LoadAssetAtPath(path, typeof(T));
#endif
        }

        protected static string savePath = "Assets/ABSystem/config.asset";

        protected AssetBundleBuildConfig _config;
        protected ReorderableList _list;        

        protected AssetBundleBuildPanel()
        {

        }

        protected virtual void OnListElementGUI(Rect rect, int index, bool isactive, bool isfocused)
        {
            const float GAP = 5;

            AssetBundleFilter filter = _config.filters[index];
            rect.y++;

            Rect r = rect;
            r.width = 16;
            r.height = 18;
            filter.valid = GUI.Toggle(r, filter.valid, GUIContent.none);

            r.xMin = r.xMax + GAP;
            r.xMax = rect.xMax - 300;
            GUI.enabled = false;
            filter.path = GUI.TextField(r, filter.path);
            GUI.enabled = true;

            r.xMin = r.xMax + GAP;
            r.width = 50;
            if (GUI.Button(r, "Select"))
            {
                string dataPath = Application.dataPath;
                string selectedPath = EditorUtility.OpenFolderPanel("Path", dataPath, "");
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    if (selectedPath.StartsWith(dataPath))
                    {
                        filter.path = "Assets/" + selectedPath.Substring(dataPath.Length + 1);
                    }
                    else
                    {
                        ShowNotification(new GUIContent("不能在Assets目录之外!"));
                    }
                }
            }

            r.xMin = r.xMax + GAP;
            r.xMax = rect.xMax;
            filter.filter = GUI.TextField(r, filter.filter);
        }

        protected virtual void OnListHeaderGUI(Rect rect)
        {
            EditorGUI.LabelField(rect, "Asset Filter");
        }

        protected virtual void OnGUI()
        {
            bool execBuild = false;
            if (_config == null)
            {
                _config = LoadAssetAtPath<AssetBundleBuildConfig>(savePath);
                if (_config == null)
                {
                    _config = new AssetBundleBuildConfig();
                }
            }

            if (_list == null)
            {
                _list = new ReorderableList(_config.filters, typeof(AssetBundleFilter));
                _list.drawElementCallback = OnListElementGUI;
                _list.drawHeaderCallback = OnListHeaderGUI;
                _list.draggable = true;
                _list.elementHeight = 22;
            }

            //tool bar
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                if (GUILayout.Button("Add", EditorStyles.toolbarButton))
                {
                    _config.filters.Add(new AssetBundleFilter());
                }
                if (GUILayout.Button("Save", EditorStyles.toolbarButton))
                {
                    Save();
                }
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Build", EditorStyles.toolbarButton))
                {
                    execBuild = true;
                }
            }
            GUILayout.EndHorizontal();

            //context
            GUILayout.BeginVertical();

            //format
            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.PrefixLabel("DepInfoFileFormat");
                _config.depInfoFileFormat = (AssetBundleBuildConfig.Format)EditorGUILayout.EnumPopup(_config.depInfoFileFormat);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);
            
            _list.DoLayoutList();
            GUILayout.EndVertical();

            //set dirty
            if (GUI.changed)
                EditorUtility.SetDirty(_config);

            if (execBuild)
                Build();
        }

        protected void Build()
        {
            Save();
            BuildAssetBundles();
        }

        protected virtual void Save()
        {
            AssetBundlePathResolver.instance = new AssetBundlePathResolver();

            if (LoadAssetAtPath<AssetBundleBuildConfig>(savePath) == null)
            {
                AssetDatabase.CreateAsset(_config, savePath);
            }
            else
            {
                EditorUtility.SetDirty(_config);
            }
        }
    }
}