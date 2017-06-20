﻿using System.Collections;
using System.IO;
using UnityEngine;

namespace KK.Frame.Loader.ABS
{
    /// <summary>
    /// Loader 父类
    /// </summary>
    public abstract class AssetBundleLoader
    {
        internal AssetBundleManager.LoadAssetCompleteHandler onComplete;

        public string bundleName;
        public AssetBundleData bundleData;
        public AssetBundleInfo bundleInfo;
        public AssetBundleManager bundleManager;
        public LoadState state = LoadState.State_None;
        public AssetBundleLoader[] depLoaders;
        
        public virtual void Load()
        {

        }

        /// <summary>
        /// 其它都准备好了，加载AssetBundle
        /// 注意：这个方法只能被 AssetBundleManager 调用
        /// 由 Manager 统一分配加载时机，防止加载过卡
        /// </summary>
        public virtual void LoadBundle()
        {

        }

        public virtual bool isComplete
        {
            get
            {
                return state == LoadState.State_Error || state == LoadState.State_Complete;
            }
        }

        protected virtual void Complete()
        {
            if (onComplete != null)
            {
                var handler = onComplete;
                onComplete = null;
                handler(bundleInfo);
            }
            bundleManager.LoadComplete(this);
        }

        protected virtual void Error()
        {
            if (onComplete != null)
            {
                var handler = onComplete;
                onComplete = null;
                handler(bundleInfo);
            }
            bundleManager.LoadError(this);
        }
    }

    /// <summary>
    /// 在手机运行时加载
    /// </summary>
    public class MobileAssetBundleLoader : AssetBundleLoader
    {
        protected int _currentLoadingDepCount;
        protected AssetBundle _bundle;
        protected bool _hasError;
        protected string _assetBundleCachedFile;
        protected string _assetBundleSourceFile;

        /// <summary>
        /// 开始加载
        /// </summary>
        override public void Load()
        {
            if (_hasError)
                state = LoadState.State_Error;

            if (state == LoadState.State_None)
            {
                state = LoadState.State_Loading;

                this.LoadDepends();
            }
            else if (state == LoadState.State_Error)
            {
                this.Error();
            }
            else if (state == LoadState.State_Complete)
            {
                this.Complete();
            }
        }

        void LoadDepends()
        {
            if (depLoaders == null)
            {
                depLoaders = new AssetBundleLoader[bundleData.dependencies.Length];
                for (int i = 0; i < bundleData.dependencies.Length; i++)
                {
                    depLoaders[i] = bundleManager.CreateLoader(bundleData.dependencies[i]);
                }
            }

            _currentLoadingDepCount = 0;
            for (int i = 0; i < depLoaders.Length; i++)
            {
                AssetBundleLoader depLoader = depLoaders[i];
                if (depLoader.state != LoadState.State_Error && depLoader.state != LoadState.State_Complete)
                {
                    _currentLoadingDepCount++;
                    depLoader.onComplete += OnDepComplete;
                    depLoader.Load();
                }
            }
            this.CheckDepComplete();
        }

        /// <summary>
        /// 其它都准备好了，加载AssetBundle
        /// 注意：这个方法只能被 AssetBundleManager 调用
        /// 由 Manager 统一分配加载时机，防止加载过卡
        /// </summary>
        override public void LoadBundle()
        {
            _assetBundleCachedFile = string.Format("{0}/{1}", bundleManager.pathResolver.BundleCacheDir, bundleName);
            _assetBundleSourceFile = bundleManager.pathResolver.GetBundleSourceFile(bundleName);

            if (File.Exists(_assetBundleCachedFile))
                bundleManager.StartCoroutine(LoadFromCachedFile());
            else
                bundleManager.StartCoroutine(LoadFromPackage());
        }

        /// <summary>
        /// 从已缓存的文件里加载
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator LoadFromCachedFile()
        {
            if (state != LoadState.State_Error)
            {
                //兼容低版本API
#if UNITY_4 || UNITY_5_1 || UNITY_5_2
                _bundle = AssetBundle.CreateFromFile(_assetBundleCachedFile);
                yield return null;
#else
                AssetBundleCreateRequest req = AssetBundle.LoadFromFileAsync(_assetBundleCachedFile);
                yield return req;
                _bundle = req.assetBundle;
#endif

                this.Complete();
            }
        }

        /// <summary>
        /// 从源文件(安装包里)加载
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator LoadFromPackage()
        {
            if (state != LoadState.State_Error)
            {
                //加载主体
                WWW www = new WWW(_assetBundleSourceFile);
                yield return www;

                //加载完缓存一份，便于下次快速加载
                if (www.error == null)
                {
                    File.WriteAllBytes(_assetBundleCachedFile, www.bytes);

                    _bundle = www.assetBundle;
                }

                www.Dispose();
                www = null;

                this.Complete();
            }
        }

        void OnDepComplete(AssetBundleInfo abi)
        {
            _currentLoadingDepCount--;
            this.CheckDepComplete();
        }

        void CheckDepComplete()
        {
            if (_currentLoadingDepCount == 0)
            {
                bundleManager.RequestLoadBundle(this);
            }
        }

        override protected void Complete()
        {
            if (bundleInfo == null)
            {
                this.state = LoadState.State_Complete;

                this.bundleInfo = bundleManager.CreateBundleInfo(this, null, _bundle);
                this.bundleInfo.isReady = true;
                this.bundleInfo.onUnloaded = OnBundleUnload;
                foreach (AssetBundleLoader depLoader in depLoaders)
                {
                    bundleInfo.AddDependency(depLoader.bundleInfo);
                }

                _bundle = null;
            }
            base.Complete();
        }

        private void OnBundleUnload(AssetBundleInfo abi)
        {
            this.bundleInfo = null;
            this.state = LoadState.State_None;
        }

        override protected void Error()
        {
            _hasError = true;
            this.state = LoadState.State_Error;
            this.bundleInfo = null;
            base.Error();
        }
    }
}
