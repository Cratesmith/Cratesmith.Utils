#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class EditorWWW : IDisposable
{
    [Serializable]
    public class WWWEvent : UnityEvent<WWW> {}

    [SerializeField] private WWW m_www;
    [SerializeField] private WWWEvent m_onComplete = new WWWEvent();
    public WWW www {get { return m_www; }}

    public EditorWWW()
    {
    // so unity serializer can construct
    }

    public EditorWWW(string url, UnityAction<WWW> onComplete)
    {
        EditorApplication.update += Update;
        m_onComplete.AddListener(onComplete);
        m_www = new WWW(url);
    }

    private void Update()
    {
        if (!m_www.isDone) return;
        m_onComplete.Invoke(m_www);
        EditorApplication.update -= Update;
    }

    public void Stop()
    {
        EditorApplication.update -= Update;
    }

    public void Dispose()
    {
        Stop();
        if (m_www != null) m_www.Dispose();
    }
}
#endif