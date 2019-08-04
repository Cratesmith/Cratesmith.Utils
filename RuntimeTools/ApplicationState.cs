using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Provides information about the application's state. Such as if it's quitting
/// </summary>
public static class ApplicationState
{
    public static bool isQuittingOrNotPlaying { get {return isQuitting || !isPlaying;}}
    public static bool isPlaying { get; private set; } 
	public static bool isQuitting { get; private set; }
	
	[RuntimeInitializeOnLoadMethod]
	static void Init()
	{
		Application.quitting += ApplicationOnQuitting;
	    isPlaying = Application.isPlaying; // cache Application.isPlaying as it can't be accessed from other threads
	}

	private static void ApplicationOnQuitting()
	{
		isQuitting = true;
	}
}

