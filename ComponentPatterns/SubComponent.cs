using System.Collections.Generic;
using UnityEngine;

namespace Cratesmith.Utils
{
    public class SubComponent<T> : MonoBehaviour
    {
        private T m_owner;
        public T owner 
        {
            get 
            {
                if(!isCached)
                {
                    m_owner = FindOwner();
//                Assert.IsTrue(isCached, string.Format("Subcomponent<{0}> {1} does not have an owner!", typeof(T), name));
                }
                return m_owner;
            }
        }

        protected virtual T FindOwner()
        {
            return GetComponentInParent<T>(); 
        }

        private bool isCached { get { return !EqualityComparer<T>.Default.Equals(m_owner, default(T));  }}
    }
}