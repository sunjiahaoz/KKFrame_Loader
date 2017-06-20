using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using KK.Frame.Loader.ABS;
using System;

namespace KK.Frame.Loader
{
    public class Loader_ABS : ILoader
    {
        Dictionary<string, WeakReference> _dictWeakRef = new Dictionary<string, WeakReference>();
        // 获得或生成一个用于标记引用的对象
        UnityEngine.Object GetUseObj(string strModuleName)
        {
            if (strModuleName.Length == 0)
            {
                strModuleName = "GLOBAL";
            }
            if (!_dictWeakRef.ContainsKey(strModuleName)
                || _dictWeakRef[strModuleName].Target as UnityEngine.Object == null)
            {
                GameObject obj = new GameObject("ABRefObj_" + strModuleName);
                _dictWeakRef[strModuleName] = new WeakReference(obj);
            }
            return _dictWeakRef[strModuleName].Target as UnityEngine.Object;
        }
        string GetBundlePath(string strModuleName, string strResTypeFolder, string relativeFilePath, string strResExt)
        {
            strModuleName = strModuleName.Replace('/', '.');
            strResTypeFolder = strResTypeFolder.Replace('/', '.');
            relativeFilePath = relativeFilePath.Replace('/', '.');
            string strPath = string.Format("Assets.ForZip{0}{1}{2}.{3}",
                strModuleName.Length > 0 ? ("." + strModuleName) : "",
                strResTypeFolder.Length > 0 ? ("." + strResTypeFolder) : "",
                relativeFilePath.Length > 0 ? ("." + relativeFilePath) : "",
                strResExt);
            string strRes = strPath.Replace("..", ".");
            return strRes;
        }

        public void Init(Action done)
        {
            AssetBundleManager.Instance.Init(done);
        }

        public void LoadMaterial(string modulenName, string relativeFilePath, Action<Material> back)
        {
            LoadMaterial(modulenName, relativeFilePath, (res, param) => { back(res); }, null);
        }
        public void LoadSprite(string moduleName, string relativeFilePath, Action<Sprite> back)
        {
            LoadSprite(moduleName, relativeFilePath, (res, param) => { back(res); }, null);
        }
        public void LoadRealSprite(string moduleName, string relativeFilePath, Action<Sprite> back)
        {
            LoadRealSprite(moduleName, relativeFilePath, (res, param) => { back(res); }, null);
        }
        public void LoadTexture(string moduleName, string relativeFilePath, Action<Texture> back)
        {
            LoadTexture(moduleName, relativeFilePath, (res, param) => { back(res); }, null);
        }
        public void LoadMultipleTexture(string moduleName, string relativeFilePath, Action<UnityEngine.Object[]> back)
        {
            LoadMultipleTexture(moduleName, relativeFilePath, (res, param) => { back(res); }, null);
        }
        public void LoadPrefab(string moduleName, string relativeFilePath, Action<GameObject> back, bool bInstantiate = true)
        {
            LoadPrefab(moduleName, relativeFilePath, (res, param) => { back(res); }, bInstantiate, null);
        }
        public void LoadScene(string moduleName, string relativeFilePath, string strSceneName, Action<bool> back)
        {
            LoadScene(moduleName, relativeFilePath, strSceneName, (res, param) => { back(res); }, null);
        }
        public void LoadSceneAsync(string moduleName, string relativeFilePath, string strSceneName, Action<AsyncOperation> back)
        {
            LoadSceneAsync(moduleName, relativeFilePath, strSceneName, (res, param) => { back(res); }, null);
        }


        public void LoadMaterial(string modulenName, string relativeFilePath, Action<Material, System.Object> back, System.Object objParam = null)
        {
            string strBundlePath = GetBundlePath(modulenName, "Material", relativeFilePath, "mat");
            AssetBundleManager.Instance.Load(strBundlePath, (info) =>
            {
                if (back == null)
                {
                    return;
                }

                if (info != null)
                {
                    back(info.Require<Material>(GetUseObj(modulenName)), objParam);
                }
                else
                {
                    Debug.LogError("<color=red>[Error]</color>---找不到资源：" + strBundlePath);
                    back(null, objParam);
                }
            });
        }

        public void LoadSprite(string moduleName, string relativeFilePath, Action<Sprite, System.Object> back, System.Object objParam = null)
        {
            string strBundlePath = GetBundlePath(moduleName, "Texture", relativeFilePath, "png");
            AssetBundleManager.Instance.Load(strBundlePath, (info) =>
            {
                if (back == null)
                {
                    return;
                }

                if (info != null)
                {
                    back(info.Require<Sprite>(GetUseObj(moduleName)), objParam);
                }
                else
                {
                    Debug.LogError("<color=red>[Error]</color>---找不到资源：" + strBundlePath);
                    back(null, objParam);
                }
            });
        }

        public void LoadRealSprite(string moduleName, string relativeFilePath, Action<Sprite, System.Object> back, System.Object objParam = null)
        {
            string strBundlePath;
            if (relativeFilePath.Contains("BG"))
            {
                strBundlePath = GetBundlePath(moduleName, "Sprite", relativeFilePath, "jpg");
            }
            else
            {
                strBundlePath = GetBundlePath(moduleName, "Sprite", relativeFilePath, "png");
            }
            AssetBundleManager.Instance.Load(strBundlePath, (info) =>
            {
                if (back == null)
                {
                    return;
                }

                if (info != null)
                {
                    back(info.Require<Sprite>(GetUseObj(moduleName)), objParam);
                }
                else
                {
                    Debug.LogError("<color=red>[Error]</color>---找不到资源：" + strBundlePath);
                    back(null, objParam);
                }
            });
        }

        public void LoadTexture(string moduleName, string relativeFilePath, Action<Texture, System.Object> back, System.Object objParam = null)
        {
            string strBundlePath = GetBundlePath(moduleName, "Texture", relativeFilePath, "png");
            AssetBundleManager.Instance.Load(strBundlePath, (info) =>
            {
                if (back == null)
                {
                    return;
                }

                if (info != null)
                {
                    back(info.Require<Texture>(GetUseObj(moduleName)), objParam);
                }
                else
                {
                    Debug.LogError("<color=red>[Error]</color>---找不到资源：" + strBundlePath);
                    back(null, objParam);
                }
            });
        }

        public void LoadMultipleTexture(string moduleName, string relativeFilePath, Action<UnityEngine.Object[], System.Object> back, System.Object objParam = null)
        {
            string strBundlePath = GetBundlePath(moduleName, "Texture", relativeFilePath, "png");
            AssetBundleManager.Instance.Load(strBundlePath, (info) =>
            {
                if (back == null)
                {
                    return;
                }

                if (info != null)
                {                    
                    back(info.LoadAllAssets(GetUseObj(moduleName)), objParam);
                }
                else
                {
                    Debug.LogError("<color=red>[Error]</color>---找不到资源：" + strBundlePath);
                    back(null, objParam);
                }
            });
        }

        public void LoadPrefab(string moduleName, string relativeFilePath, Action<GameObject, System.Object> back, bool bInstantiate = true, System.Object objParam = null)
        {
            string strBundlePath = GetBundlePath(moduleName, "Prefab", relativeFilePath, "prefab");
            AssetBundleManager.Instance.Load(strBundlePath, (info) =>
            {
                if (back == null)
                {
                    return;
                }

                if (info != null)
                {
                    if (bInstantiate)
                    {
                    //back(info.Instantiate(), objParam);
                    GameObject go = PoolableObject.Instantiate(info.Require<GameObject>(GetUseObj(moduleName)));
                        back(go, objParam);
                    }
                    else
                    {
                        back(info.Require<GameObject>(GetUseObj(moduleName)), objParam);
                    }
                }
                else
                {
                    Debug.LogError("<color=red>[Error]</color>---找不到资源：" + strBundlePath);
                    back(null, objParam);
                }
            });
        }

        public void LoadScene(string moduleName, string relativeFilePath, string strSceneName, Action<bool, System.Object> back, System.Object objParam = null)
        {
            string strBundlePath = GetBundlePath(moduleName, "Scene", relativeFilePath + "/" + strSceneName, "unity");
            AssetBundleManager.Instance.Load(strBundlePath, (info) =>
            {
                info.Require(GetUseObj(moduleName));
                if (back == null)
                {
                    return;
                }

                if (info != null)
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene(strSceneName);
                    back(true, objParam);
                }
                else
                {
                    Debug.LogError("<color=red>[Error]</color>---找不到资源：" + strBundlePath);
                    back(false, objParam);
                }
            });
        }

        public void LoadSceneAsync(string moduleName, string relativeFilePath, string strSceneName, Action<AsyncOperation, System.Object> back, System.Object objParam = null)
        {
            string strBundlePath = GetBundlePath(moduleName, "Scene", relativeFilePath + "/" + strSceneName, "unity");
            AssetBundleManager.Instance.Load(strBundlePath, (info) =>
            {
                info.Retain(GetUseObj("")); // 场景使用GLOBAL
            if (back == null)
                {
                    return;
                }

                if (info != null)
                {
                    AsyncOperation ao = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(strSceneName);
                    back(ao, objParam);
                }
                else
                {
                    Debug.LogError("<color=red>[Error]</color>---找不到资源：" + strBundlePath);
                    back(null, objParam);
                }
            });
        }
    }
}
