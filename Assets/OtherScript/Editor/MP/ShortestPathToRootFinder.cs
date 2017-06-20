using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MemoryProfilerWindow;
using UnityEditor.MemoryProfiler;
using UnityEngine;

namespace MemoryProfilerWindow
{
    class ShortestPathToRootFinder
    {
        private readonly CrawledMemorySnapshot _snapshot;

        public ShortestPathToRootFinder(CrawledMemorySnapshot snapshot)
        {
            _snapshot = snapshot;
        }

        public ThingInMemory[] FindFor(ThingInMemory thing)
        {
            var seen = new HashSet<ThingInMemory>();
            var queue = new Queue<List<ThingInMemory>>();
            queue.Enqueue(new List<ThingInMemory> { thing});

            while (queue.Any())
            {
                var pop = queue.Dequeue();
                var tip = pop.Last();

                string reason;
                if (IsRoot(tip, out reason))
                    return pop.ToArray();

                foreach (var next in tip.referencedBy)
                {
                    if (seen.Contains(next))
                        continue;
                    seen.Add(next);
                    var dupe = new List<ThingInMemory>(pop) {next};
                    queue.Enqueue(dupe);
                }
            }
            return null;
        }

        public bool IsRoot(ThingInMemory thing, out string reason)
        {
            reason = null;
            if (thing is StaticFields)
            {
                reason = "Static fields are global variables. Anything they reference will not be unloaded.";
                return true;
            }
            if (thing is ManagedObject)
                return false;
            if (thing is GCHandle)
                return false;

            var nativeObject = thing as NativeUnityEngineObject;
            if (nativeObject == null)
                throw new ArgumentException("Unknown type: " + thing.GetType());
            if (nativeObject.isManager)
            {
                reason = "this is an internal unity'manager' style object, which is a global object that will never be unloaded";
                return true;
            }
            if (nativeObject.isDontDestroyOnLoad)
            {
                reason = "DontDestroyOnLoad() was called on this object, so it will never be unloaded";
                return true;
            }

            if ((nativeObject.hideFlags & HideFlags.DontUnloadUnusedAsset) != 0)
            {
                reason = "the DontUnloadUnusedAsset hideflag is set on this object. Unity's builtin resources set this flag. Users can also set the flag themselves";
                return true;
            }

            if (nativeObject.isPersistent)
                return false;

            if (IsComponent(nativeObject.classID))
            {
                reason = "this is a component, living on a gameobject, that is either part of the loaded scene, or was generated by script. It will be unloaded on next scene load.";
                return true;
            }

            if (IsGameObject(nativeObject.classID))
            {
                reason = "this is a gameobject, that is either part of the loaded scene, or was generated by script. It will be unloaded on next scene load if nobody is referencing it";
                return true;
            }

            if (IsAssetBundle(nativeObject.classID))
            {
                reason = "this object is an assetbundle, which is never unloaded automatically, but only through an explicit .Unload() call.";
                return true;
            }

            reason = "This object is a root, but the memory profiler UI does not yet understand why";
            return true;
        }

        private bool IsGameObject(int classID)
        {
            return _snapshot.nativeTypes[classID].name == "GameObject";
        }

        private bool IsAssetBundle(int classID)
        {
            return _snapshot.nativeTypes[classID].name == "AssetBundle";
        }

        private bool IsComponent(int classID)
        {
            var packedNativeType = _snapshot.nativeTypes[classID];

            if (packedNativeType.name == "Component")
                return true;

            var baseClassID = packedNativeType.baseClassId;

            return baseClassID != -1 && IsComponent(baseClassID);
        }
    }
}
