using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public abstract class PeerComponent<T> : MonoBehaviour
{
    private T m_owner;
    public T owner
    {
        get
        {
            if (!isCached)
            {
                m_owner = GetComponent<T>();
                Assert.IsTrue(isCached, string.Format("Subcomponent<{0}> {1} does not have an owner!", typeof(T), name));
            }
            return m_owner;
        }
    }

    private bool isCached { get { return !EqualityComparer<T>.Default.Equals(m_owner, default(T)); } }
}
