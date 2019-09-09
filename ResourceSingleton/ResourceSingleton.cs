//#define LOG_DEBUG

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
#endif

namespace Cratesmith.Utils
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class ResourceFilenameAttribute : Attribute
    {
        private string filename;
        private bool isSuffix;
        static Dictionary<Type,string> s_cache = new Dictionary<Type, string>();

        public ResourceFilenameAttribute(string filename, bool isSuffix=false)
        {
            this.filename = filename;
            this.isSuffix = isSuffix;
        } 

        public static string Get<T>()
        {        
            return Get(typeof(T));
        }

        public static string Get(Type type)
        {
            var attribs = (ResourceFilenameAttribute[])type.GetCustomAttributes(typeof(ResourceFilenameAttribute), true);
            string output;
            if (!s_cache.TryGetValue(type, out output))
            {
                if (attribs.Length > 0)
                {
                    var attrib = attribs[0];
                    output = !attrib.isSuffix
                        ? attrib.filename
                        : type.Name + attrib.filename;
                }
                else
                {
                    output = type.Name;
                }

                s_cache[type] = output;
            }
              
            return output;
        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class ResourceDirectoryAttribute : Attribute
    {
        private string directoryName;
        private bool isSuffix;

        public ResourceDirectoryAttribute(string directoryName, bool isSuffix=false)
        {
            this.directoryName = directoryName;
            this.isSuffix = isSuffix;
        } 
#if UNITY_EDITOR
        public static string Get(MonoScript script)
        {
            if (script == null)
            {
                return string.Empty;
            }

            var type = script.GetClass();
            if (type == null)
            {
                return string.Empty;
            }

            var attribs = (ResourceDirectoryAttribute[])type.GetCustomAttributes(typeof(ResourceDirectoryAttribute), true);

            string output;
            var scriptDir = Path.GetDirectoryName(AssetDatabase.GetAssetPath(script));
            if (attribs.Length > 0)
            {
                var attrib = attribs[0];
                output = !attrib.isSuffix
                    ? attrib.directoryName
                    : scriptDir + "/" + attrib.directoryName;
            }
            else
            {
                output = scriptDir;
            }
              
            return output;
        }
#endif
    }

    public abstract class ResourceSingletonBase : ScriptableObject
    {
#if UNITY_EDITOR
        public virtual void OnRebuildInEditor()
        {                
        }
#endif
    }

    public abstract class ResourceSingleton<T> : ResourceSingletonBase where T:ResourceSingleton<T>
    {
        static T s_instance;

        protected static T instance
        {
            get 
            { 
                LoadAsset();

                if(s_instance==null)
                {
                    throw new ArgumentNullException("Couldn't load asset for ResourceSingleton "+typeof(T).Name);
                }

                return s_instance; 
            } 
        }

#if UNITY_EDITOR
        protected static T rawInstance
        {
            get 
            { 
                LoadAsset();
                return s_instance; 
            } 
        }
#endif

        static void LoadAsset()
        {
            if(Application.isPlaying)
            {
                if(!s_instance)
                {
                    s_instance = Resources.Load(ResourceFilenameAttribute.Get<T>()) as T;
                }
            }
#if UNITY_EDITOR
            if(!s_instance)
            {
                s_instance = ResourceSingletonBuilder.BuildOrMoveAsset(typeof(T)) as T;            
            }
#endif
        }
    }

    #region internal
#if UNITY_EDITOR
    public class ResourceSingletonBuilder : AssetPostprocessor
    {
        private static Queue<Type> s_stepQueue;
        
        public class Builder : AssetPostprocessor
        {
            const string EDITORPREFS_KEY_ModifiedPaths = "ResourceSingleton.Builder.ModifiedPaths";
            [Serializable] public class PathList
            {
                public string[] values;
            } 
            static string[] ModifiedPaths
            {
                set
                {
                    var json = JsonUtility.ToJson(new PathList {values = value});
                    EditorPrefs.SetString(EDITORPREFS_KEY_ModifiedPaths,json);
                }
                get
                {
                    var json = EditorPrefs.GetString(EDITORPREFS_KEY_ModifiedPaths);
                    var data = JsonUtility.FromJson<PathList>(json);
                    return data?.values != null ? data.values:new string[0];
                }
            }

            static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
                string[] movedFromAssetPaths)
            {
                ModifiedPaths = importedAssets
                    .Concat(movedAssets)
                    .Where(x => AssetImporter.GetAtPath(x) as MonoImporter)
                    .ToArray();
            }
            
            [DidReloadScripts(-100)]
            public static void OnScriptsReloaded()
            {
                var modifiedPaths = ModifiedPaths;
                if (modifiedPaths.Length == 0) return;
                
                BuildResourceSingletons(modifiedPaths);
                ModifiedPaths = new string[0];
            } 
        }

        public static void BuildResourceSingletons(string[] modifiedPaths)
        {
#if LOG_DEBUG
	    Debug.Log("Building ResourceSingeltons");
#endif	    
            var modifiedSingletonClasses = modifiedPaths
                .Select(x=>
                {
                    var script = AssetDatabase.LoadAssetAtPath<MonoScript>(AssetDatabase.GUIDToAssetPath(x));
                    return script?.GetClass();
                }).Where(t => t!=null && !t.IsAbstract && GetBaseType(t, typeof(ResourceSingleton<>)));

            if (EditorApplication.isPlayingOrWillChangePlaymode || BuildPipeline.isBuildingPlayer)
            {
                foreach(var i in modifiedSingletonClasses)
                {  
                    BuildOrMoveAsset(i);
                }
            }
            else
            {
                s_stepQueue = new Queue<Type>(modifiedSingletonClasses);
                EditorApplication.delayCall += BuildResourceSingletons_Step;
            }
        }

        private static void BuildResourceSingletons_Step()
        {
            const int PER_STEP = 5;
            for (int i = 0; i < PER_STEP && s_stepQueue.Count > 0; i++)
            {
                var type = s_stepQueue.Dequeue();
                BuildOrMoveAsset(type);
            }
		
            if (s_stepQueue.Count > 0)
            {
                EditorApplication.delayCall += BuildResourceSingletons_Step;
            }
        }

        static bool GetBaseType(Type type, Type baseType)
        {
            if (type == null || baseType == null || type == baseType)
                return false;

            if (baseType.IsGenericType == false)
            {
                if (type.IsGenericType == false)
                    return type.IsSubclassOf(baseType);
            }
            else
            {
                baseType = baseType.GetGenericTypeDefinition();
            }

            type = type.BaseType;
            Type objectType = typeof(object);
            while (type != objectType && type != null)
            {
                Type curentType = type.IsGenericType ?
                    type.GetGenericTypeDefinition() : type;
                if (curentType == baseType)
                    return true;

                type = type.BaseType;
            }
              
            return false;
        }

        public static UnityEngine.Object BuildOrMoveAsset(Type type) 
        {
#if LOG_DEBUG
	        Debug.Log("Building ResourceSingleton: " + type.FullName);
#endif
            var editorPrefsKey = "ResourceSingleton.PrevFilename." + type.FullName;
            var resourceFilename = ResourceFilenameAttribute.Get(type);
            var instance = Resources.Load(resourceFilename, type) as ResourceSingletonBase;
            var prevFilename = EditorPrefs.GetString(editorPrefsKey);
            if (instance == null && !string.IsNullOrEmpty(prevFilename))
            {
                instance = Resources.Load(prevFilename, type) as ResourceSingletonBase;
            }
            EditorPrefs.SetString("ResourceSingleton.PrevFilename."+type.FullName, resourceFilename);

            var temp = ScriptableObject.CreateInstance(type);
            var monoscript = MonoScript.FromScriptableObject(temp);

            ScriptableObject.DestroyImmediate(temp);
            if(monoscript==null)
            {
                Debug.LogError("Couldn't find script named "+type.Name+".cs (monoscripts must be in a file named the same as their class)");
                return null;
            } 
        
            var scriptPath = ResourceDirectoryAttribute.Get(monoscript);
            var assetDir = (scriptPath+"/Resources/").Replace("\\","/");

            var assetPath  = assetDir+resourceFilename+".asset";
            Directory.CreateDirectory(assetDir);

            if(instance && AssetDatabase.GetAssetPath(instance)!=assetPath)
            {
                if (!File.Exists(assetPath))
                {
                    Debug.Log("ResourceSingleton: Moving asset: " + type.Name + " from " +
                              AssetDatabase.GetAssetPath(instance) + " to " + assetPath);
                    FileUtil.MoveFileOrDirectory(AssetDatabase.GetAssetPath(instance), assetPath);
                    AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                    AssetDatabase.ImportAsset(assetPath,ImportAssetOptions.ForceSynchronousImport);
                    instance = AssetDatabase.LoadAssetAtPath(assetPath, type) as ResourceSingletonBase;
                }
                else
                {
                    Debug.LogWarning("ResourceSingleton: Didn't move asset: " + type.Name + " from " +
                                     AssetDatabase.GetAssetPath(instance) + " to " + assetPath+ " as a file already exists there!");                    
                }
            }

            if(!instance && !File.Exists(assetPath))
            {
                Debug.Log("ResourceSingleton: Creating asset: " + type.Name + " at " + assetPath);
                instance = ScriptableObject.CreateInstance(type) as ResourceSingletonBase;
                AssetDatabase.CreateAsset(instance, assetPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.ImportAsset(assetPath,ImportAssetOptions.ForceSynchronousImport);
                instance = AssetDatabase.LoadAssetAtPath(assetPath, type) as ResourceSingletonBase;
            }
            
            if (instance)
            {			
                type.GetField("s_instance", BindingFlags.NonPublic|BindingFlags.Static).SetValue(null, instance);
                instance.OnRebuildInEditor();            
            }

            return instance;
        }
    }
#endif
    #endregion
}