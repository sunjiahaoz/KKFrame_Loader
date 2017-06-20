using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KK.Frame.Util.Editor
{
    /// <summary>
    /// 编辑器下常用工具方法
    /// </summary>
    class ToolsEditor
    {
        /// <summary>
        /// 添加宏
        /// 多个宏请用分号隔开
        /// </summary>
        /// <param name="strSym"></param>
        public static void AddDefineSymble(string strSym)
        {
            UnityEditor.BuildTargetGroup btg = UnityEditor.BuildTargetGroup.Unknown;
#if UNITY_ANDROID
            btg = UnityEditor.BuildTargetGroup.Android;
#elif UNITY_IPHONE
            btg = UnityEditor.BuildTargetGroup.IOS;
#elif UNITY_EDTIRO
            btg = UnityEditor.BuildTargetGroup.Standalone;
#endif
            string strDef = UnityEditor.PlayerSettings.GetScriptingDefineSymbolsForGroup(btg);

            if (strDef.IndexOf(strSym) > 0)
            {
                return;
            }

            if (strDef.Length > 0)
            {
                strDef += ";" + strSym;
            }
            else
            {
                strDef = strSym;
            }
            UnityEditor.PlayerSettings.SetScriptingDefineSymbolsForGroup(btg, strDef);
        }

        /// <summary>
        /// 移除宏
        /// 要移除多个宏请用分号分隔
        /// </summary>
        /// <param name="strSym"></param>
        public static void RemoveDefineSymble(string strSym)
        {
            UnityEditor.BuildTargetGroup btg = UnityEditor.BuildTargetGroup.Unknown;
#if UNITY_ANDROID
            btg = UnityEditor.BuildTargetGroup.Android;
#elif UNITY_IPHONE
            btg = UnityEditor.BuildTargetGroup.IOS;
#elif UNITY_EDTIRO
            btg = UnityEditor.BuildTargetGroup.Standalone;
#endif
            string strDef = UnityEditor.PlayerSettings.GetScriptingDefineSymbolsForGroup(btg);
            string[] strSyms = strSym.Split(';');
            for (int i = 0; i < strSyms.Length; ++i)
            {
                if (strSyms[i].Length == 0)
                {
                    continue;
                }

                strDef = strDef.Replace(strSyms[i] + ";", "");
                strDef = strDef.Replace(strSyms[i], "");
            }

            // 移除最后的分号
            if (strDef[strDef.Length - 1] == ';')
            {
                strDef = strDef.Remove(strDef.Length - 1, 1);
            }

            UnityEditor.PlayerSettings.SetScriptingDefineSymbolsForGroup(btg, strDef);
        }
        
    }
}
