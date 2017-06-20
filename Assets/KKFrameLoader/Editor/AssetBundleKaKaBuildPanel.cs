using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEditorInternal;
using KK.Frame.Util.Editor;

namespace KK.Frame.Loader.ABS
{
    public class AssetBundleKaKaBuildPanel : EditorWindow
    {
#if AB_MODE
        [MenuItem("ABSystem/切换到读取本地")]
        static void SwitchToLocal()
        {
            CloseABMode();
        }
#endif

#if (!AB_MODE && !USEZIP) || (AB_MODE && USEZIP)
        [MenuItem("ABSystem/切换到使用读取AB")]
        static void SwitchToAB()
        {
            OpenABMode_NoUseZip();
        }
#endif

#if (!AB_MODE && !USEZIP) || (AB_MODE && !USEZIP)
        [MenuItem("ABSystem/切换到使用读取AB于Zip压缩包")]
        static void SwitchToABZip()
        {
            OpenABMode_UseZip();
        }
#endif

        // 关闭AB模式
        static void CloseABMode()
        {
            ToolsEditor.RemoveDefineSymble("AB_MODE");
            ToolsEditor.RemoveDefineSymble("USEZIP");
        }

        // 开启AB模式并使用zip文件方式
        static void OpenABMode_UseZip()
        {
            ToolsEditor.AddDefineSymble("AB_MODE");
            ToolsEditor.AddDefineSymble("USEZIP");
        }
        // 开启AB模式并使用直接读AB包模式
        static void OpenABMode_NoUseZip()
        {
            ToolsEditor.AddDefineSymble("AB_MODE");
            ToolsEditor.RemoveDefineSymble("USEZIP");
        }

        [MenuItem("ABSystem/KaKa Builder Panel")]
        static void Open()
        {
            GetWindow<AssetBundleKaKaBuildPanel>("ABSystem KakaBuilderPanel", true);
        }

        class SelectedModuleFolder 
        {
            public SelectedModuleFolder(string strName, bool bSelected)
            {
                _strFolderName = strName;
                _bSelected = bSelected;
            }
            public string _strFolderName;
            public bool _bSelected;
        }

                
        int CoordTranslateToIndex(int nXPos, int nYPos, int nWidth)
        {
            return nYPos * nWidth + nXPos;
        }
        static bool bShowCommon = true;
        protected void OnGUI()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                if (GUILayout.Button("Common", EditorStyles.toolbarButton))
                {
                    bShowCommon = true;
                }
                if (GUILayout.Button("Filter", EditorStyles.toolbarButton))
                {
                    bShowCommon = false;
                }
            }
            GUILayout.EndHorizontal();


            if (bShowCommon)
            {
                OnGUI_Common();
            }
            else
            {
                OnGUI_Filter();
            }
        }

        #region _Filter_
        const string _filterConfigPath = "Assets/KKFrameLoader/JustUsedForFilter.asset";
        AssetBundleBuildConfig _filterConfig = null;
        protected ReorderableList _list;
        protected virtual void OnListElementGUI(Rect rect, int index, bool isactive, bool isfocused)
        {
            const float GAP = 5;

            AssetBundleFilter filter = _filterConfig.filters[index];
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
        void OnGUI_Filter()
        {
            if (_filterConfig == null)
            {
                _filterConfig = AssetBundleBuildPanel.LoadAssetAtPath<AssetBundleBuildConfig>(_filterConfigPath);
                if (_filterConfig == null)
                {
                    _filterConfig = new AssetBundleBuildConfig();
                }
            }
            if (_list == null)
            {
                _list = new ReorderableList(_filterConfig.filters, typeof(AssetBundleFilter));
                _list.drawElementCallback = OnListElementGUI;
                _list.drawHeaderCallback = OnListHeaderGUI;
                _list.draggable = true;
                _list.elementHeight = 22;
            }

            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            {                
                if (GUILayout.Button("Save", EditorStyles.toolbarButton))
                {
                    Save_Filter();
                }
            }
            GUILayout.EndHorizontal();


            //context
            GUILayout.BeginVertical();
            GUILayout.Space(10);
            _list.DoLayoutList();
            GUILayout.EndVertical();

            //set dirty
            if (GUI.changed)
                EditorUtility.SetDirty(_filterConfig);
        }

        protected virtual void Save_Filter()
        {            
            AssetBundlePathResolver.instance = new AssetBundlePathResolver();

            if (AssetBundleBuildPanel.LoadAssetAtPath<AssetBundleBuildConfig>(_filterConfigPath) == null)
            {
                AssetDatabase.CreateAsset(_filterConfig, _filterConfigPath);
            }
            else
            {
                EditorUtility.SetDirty(_filterConfig);
            }
            ShowNotification(new GUIContent("已保存到" + _filterConfigPath));
        }
        #endregion

        #region _Common_
        List<SelectedModuleFolder> _lstModuleFolder = new List<SelectedModuleFolder>();
        const string _savePath = "Assets/KKFrameLoader/config.asset";
        AssetBundleBuildConfig _config = null;
        void OnGUI_Common()
        {
            bool execBuild = false;
            if (_config == null)
            {
                _config = AssetBundleBuildPanel.LoadAssetAtPath<AssetBundleBuildConfig>(_savePath);
                if (_config == null)
                {
                    _config = new AssetBundleBuildConfig();
                }
            }

            //  todo
            OnGUI_ShowFolder();
            OnGUI_ShowFilter();

            //tool bar
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                if (GUILayout.Button("Save", EditorStyles.toolbarButton))
                {
                    Save_Config();
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

            GUILayout.Space(10);
            GUILayout.EndVertical();

            if (execBuild)
                Build();
        }
        protected void Build()
        {
            Save_Config();
            BuildAssetBundles();
        }
        void BuildAssetBundles()
        {
            AssetBundleBuildConfig config = AssetBundleBuildPanel.LoadAssetAtPath<AssetBundleBuildConfig>(_savePath);
             
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
        protected void Save_Config()
        {   
            _config.depInfoFileFormat = AssetBundleBuildConfig.Format.Text;            
            _config.filters.Clear();            
            for (int i = 0; i < _lstModuleFolder.Count; ++i)
            {
                if (!_lstModuleFolder[i]._bSelected)
                {
                    continue;
                }

                for (int j = 0; j < _filterConfig.filters.Count; ++j)
                {
                    if (!_filterConfig.filters[j].valid)
                    {
                        continue;
                    }                    
                    AssetBundleFilter filter = new AssetBundleFilter();
                    filter.valid = true;
                    filter.filter = _filterConfig.filters[j].filter;
                    filter.path = "Assets/ForZip/" + _lstModuleFolder[i]._strFolderName;

                    _config.filters.Add(filter);
                }
            }

            AssetBundlePathResolver.instance = new AssetBundlePathResolver();

            if (AssetBundleBuildPanel.LoadAssetAtPath<AssetBundleBuildConfig>(_savePath) == null)
            {
                AssetDatabase.CreateAsset(_config, _savePath);
            }
            else
            {
                EditorUtility.SetDirty(_config);
            }

            ShowNotification(new GUIContent("已保存到" + _savePath));
        }
        void LoadFolder()
        {
            if (_lstModuleFolder.Count == 0)
            {
                string[] strPaths = null;
                try
                {
                    strPaths = Directory.GetDirectories(Application.dataPath + "/ForZip", "*", SearchOption.TopDirectoryOnly);

                    for (int i = 0; i < strPaths.Length; ++i)
                    {
                        string strFolderName = Path.GetFileNameWithoutExtension(strPaths[i]);
                        _lstModuleFolder.Add(new SelectedModuleFolder(strFolderName, true));
                    }
                }
                catch (System.Exception ex)
                {
                    ShowNotification(new GUIContent("确定ForZip文件夹在" + Application.dataPath + "中！！"));
                }
            }
        }

        //是否全选文件
        bool isAllFolder = false;
        bool isNoneFolder = false;
        //是否全选文件格式
        bool isAllFilter = false;
        bool isNoneFilter = false;

        void OnGUI_ShowFolder()
        {
            LoadFolder();
            GUILayout.BeginVertical();
            GUILayout.Label("Module Folder");
            #region _2017-5-6  uncle add  增加全选和全不选按钮 _
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            if (GUILayout.Button("All",EditorStyles.miniButton, GUILayout.Width(50)))
            {
                isAllFolder = true;
            }
            GUILayout.Space(10);
            if (GUILayout.Button("None", EditorStyles.miniButton, GUILayout.Width(50)))
            {
                isNoneFolder = true;
            }
            if (isAllFolder)
            {
                isAllFolder = false;
                for (int i = 0; i < _lstModuleFolder.Count; i++)
                    _lstModuleFolder[i]._bSelected = true;
            }
            if (isNoneFolder)
            {
                isNoneFolder = false;
                for (int i = 0; i < _lstModuleFolder.Count; i++)
                    _lstModuleFolder[i]._bSelected = false;
            }
            GUILayout.EndHorizontal();
            #endregion

            int nCountPerLine = 3;
            int nLineCount = (_lstModuleFolder.Count / nCountPerLine) + 1;
            GUILayout.BeginHorizontal();
            for (int i = 0; i < nLineCount; ++i)
            {
                GUILayout.BeginVertical();
                for (int j = 0; j < nCountPerLine; ++j)
                {
                    int nIndex = CoordTranslateToIndex(j, i, nCountPerLine);
                    if (nIndex >= _lstModuleFolder.Count)
                    {
                        continue;
                    }
                    _lstModuleFolder[nIndex]._bSelected = GUILayout.Toggle(_lstModuleFolder[nIndex]._bSelected, _lstModuleFolder[nIndex]._strFolderName);
                }
                GUILayout.EndVertical();
            }
            
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }
        void LoadFilterConfig()
        {
            if (_filterConfig == null)
            {
                _filterConfig = AssetBundleBuildPanel.LoadAssetAtPath<AssetBundleBuildConfig>(_filterConfigPath);
                if (_filterConfig == null)
                {
                    _filterConfig = new AssetBundleBuildConfig();
                }
            }
        }
        void OnGUI_ShowFilter()
        {
            LoadFilterConfig();
            GUILayout.BeginVertical();
            GUILayout.Space(10);
            GUILayout.Label("Filter Select");
            #region _2017-5-6  uncle add  增加全选和全不选按钮 _
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            if (GUILayout.Button("All", EditorStyles.miniButton, GUILayout.Width(50)))
            {
                isAllFilter = true;
            }
            GUILayout.Space(10);
            if (GUILayout.Button("None", EditorStyles.miniButton, GUILayout.Width(50)))
            {
                isNoneFilter = true;
            }
            if (isAllFilter)
            {
                isAllFilter = false;
                for (int i = 0; i < _filterConfig.filters.Count; i++)
                    _filterConfig.filters[i].valid = true;
            }
            if (isNoneFilter)
            {
                isNoneFilter = false;
                for (int i = 0; i < _filterConfig.filters.Count; i++)
                    _filterConfig.filters[i].valid = false;
            }
            GUILayout.EndHorizontal();
            #endregion
            int nCountPerLine = 3;
            int nLineCount = (_filterConfig.filters.Count / nCountPerLine) + 1;
            GUILayout.BeginHorizontal();
            for (int i = 0; i < nLineCount; ++i)
            {
                GUILayout.BeginVertical();
                for (int j = 0; j < nCountPerLine; ++j)
                {
                    int nIndex = CoordTranslateToIndex(j, i, nCountPerLine);
                    if (nIndex >= _filterConfig.filters.Count)
                    {
                        continue;
                    }
                    _filterConfig.filters[nIndex].valid = GUILayout.Toggle(_filterConfig.filters[nIndex].valid, _filterConfig.filters[nIndex].filter);
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }
        #endregion
    }

}