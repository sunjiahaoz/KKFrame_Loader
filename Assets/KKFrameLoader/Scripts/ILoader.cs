using UnityEngine;
using System;
using System.Collections;

namespace KK.Frame.Loader
{
    public interface ILoader
    {
        void Init(Action done);
        void LoadMaterial(string modulenName, string relativeFilePath, Action<Material> back);
        void LoadSprite(string moduleName, string relativeFilePath, Action<Sprite> back);
        void LoadRealSprite(string moduleName, string relativeFilePath, Action<Sprite> back);
        void LoadTexture(string moduleName, string relativeFilePath, Action<Texture> back);
        void LoadMultipleTexture(string moduleName, string relativeFilePath, Action<UnityEngine.Object[]> back);        
        void LoadPrefab(string moduleName, string relativeFilePath, Action<GameObject> back, bool bInstantiate = true);
        void LoadScene(string moduleName, string relativeFilePath, string strSceneName, Action<bool> back);
        void LoadSceneAsync(string moduleName, string relativeFilePath, string strSceneName, Action<AsyncOperation> back);

        void LoadMaterial(string modulenName, string relativeFilePath, Action<Material, System.Object> back, System.Object objParam = null);
        void LoadSprite(string moduleName, string relativeFilePath, Action<Sprite, System.Object> back, System.Object objParam = null);
        void LoadRealSprite(string moduleName, string relativeFilePath, Action<Sprite, System.Object> back, System.Object objParam = null);
        void LoadTexture(string moduleName, string relativeFilePath, Action<Texture, System.Object> back, System.Object objParam = null);
        void LoadMultipleTexture(string moduleName, string relativeFilePath, Action<UnityEngine.Object[], System.Object> back, System.Object objParam = null);
        void LoadPrefab(string moduleName, string relativeFilePath, Action<GameObject, System.Object> back, bool bInstantiate = true, System.Object objParam = null);
        void LoadScene(string moduleName, string relativeFilePath, string strSceneName, Action<bool, System.Object> back, System.Object objParam = null);
        void LoadSceneAsync(string moduleName, string relativeFilePath, string strSceneName, Action<AsyncOperation, System.Object> back, System.Object objParam = null);
    }
}
