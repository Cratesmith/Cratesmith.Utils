#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public static class ScriptAssetUtil
{
	public static string MakeValidFileName( string name )
	{
		string invalidChars = System.Text.RegularExpressions.Regex.Escape( new string( System.IO.Path.GetInvalidFileNameChars() ) );
		string invalidRegStr = string.Format( @"([{0}]*\.+$)|([{0}]+)", invalidChars );

		return System.Text.RegularExpressions.Regex.Replace( name, invalidRegStr, "_" );
	}

	public static string GetCreateAssetPath(string defaultPath = "Assets")
	{
		return GetCreateAssetPathFor(Selection.assetGUIDs.FirstOrDefault(), defaultPath);
	}

	public static string GetCreateAssetPathFor(Object obj, string defaultPath = "Assets")
	{
		string guid;
		long local;
		AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj, out guid, out local);
		return GetCreateAssetPathFor(guid, defaultPath);
	}

    public static string GetCreateAssetPathFor(string guid, string defaultPath = "Assets")
    {
        var path = AssetDatabase.GUIDToAssetPath(guid);
	    if (string.IsNullOrEmpty(path))
	    {
		    path = defaultPath;
	    }

        if (!Directory.Exists(path))
        {
            path = Path.GetDirectoryName(path);
        }

        return path;
    }

    public static string GetUniqueFilename(string filename)
    {
        var dir = Path.GetDirectoryName(filename);
        var current = filename;
        int id = 0;
        while (File.Exists(current))
        {
            ++id;
            current = dir +"/"+ Path.GetFileNameWithoutExtension(filename) + id + Path.GetExtension(filename);
        }

        return Path.GetFileName(current);
    }

    public static string TrimGenericsFromType(string name)
    {
        int index = name.IndexOf('`');
        if (index == -1)
        {
            return name;
        }
        return name.Substring(0, index);
    }

    private static Dictionary<string, Texture> ____iconsTable;
    private static Dictionary<string, Texture> icons
    {
        get
        {
            if (____iconsTable == null)
            {
                ____iconsTable = AssetDatabase.GetAllAssetPaths()
                                .Where(x=>x.EndsWith(" Icon.png", StringComparison.InvariantCultureIgnoreCase))
                                .ToDictionary(
                                    k=>Path.GetFileName(k.Substring(0,k.Length-" Icon.png".Length)), 
                                    AssetDatabase.LoadAssetAtPath<Texture>
                                    );
            }
            return ____iconsTable;
        }
    }

    public static Texture GetIconForType(Type type)
    {
        while (type != null)
        {
            var name = TrimGenericsFromType(type.Name);
            var fullName = TrimGenericsFromType(type.FullName);
            Texture icon;
            if (icons.TryGetValue(name, out icon) || icons.TryGetValue(fullName, out icon))
            {
                return icon;
            }        
            type = type.BaseType;
        }

        return null;
    }

    public static void ShowCreateScriptDialog(Type type, string format, params object[] extraParams)
    {
        var typename = TrimGenericsFromType(type.Name);
        var dir = GetCreateAssetPath();
        var icon = GetIconForType(type);
        ModalTextboxWindow.Create("Create New " + typename, input =>
        {
            var filename = dir + "/" + GetUniqueFilename(dir + "/" + input + ".cs");
            var className = Path.GetFileNameWithoutExtension(filename);
            CreateScript(filename, format, className, extraParams);
        }, "New" + typename, icon);
    }

    public static void CreateScript(string filename, string format, string className, params object[] extraParams)
    {        
        var param = new[] {className}.Concat(extraParams).ToArray();
        var content = string.Format(format, param);
        File.WriteAllText(filename, content);
        EditorApplication.delayCall += () => AssetDatabase.ImportAsset(filename);
    }

    public static string GetNearestScriptDirectory(Object asset)
    {
        var assetPath = AssetDatabase.GetAssetOrScenePath(asset);  
        return GetNearestScriptDirectory(!string.IsNullOrEmpty(assetPath) ? assetPath : "Assets/Scripts");
    }

    private static string GetNearestScriptDirectory(string basePath)
    {
        var assetsDir = new DirectoryInfo("Assets");
        string[] BUNDLE_DIRECTORIES = new[] {"prefabs", "textures", "scenes", "scripts"};

        var startDir = Path.GetDirectoryName(basePath); 
        var dirInfo = new DirectoryInfo(startDir);
        while (dirInfo!=null && dirInfo != assetsDir)
        {
            dirInfo = dirInfo.Parent;

            var existingScriptDir = dirInfo.GetDirectories().FirstOrDefault(x => x.Name.ToLower() == "scripts");
            if (existingScriptDir != null)
            {
                dirInfo = existingScriptDir;
                break;
            }

            if (System.Array.IndexOf(BUNDLE_DIRECTORIES, dirInfo.Name.ToLower()) != -1)
            {
                dirInfo = dirInfo.Parent.CreateSubdirectory("Scripts");
                break;
            }
        }

        if (dirInfo == null || dirInfo == assetsDir)
        {
            return "Assets/Scripts";
        }

        return "Assets" + dirInfo.FullName.Substring(Application.dataPath.Length).Replace('\\','/');
    }
}
#endif