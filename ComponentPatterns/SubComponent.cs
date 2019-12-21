using System;
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
                if(EqualityComparer<T>.Default.Equals(m_owner, default(T)))
                {
                    m_owner = GetComponentInParent<T>(); 
                }
                return m_owner;
            }
        }

        protected virtual void OnDespawn()
        {
            m_owner = default;
        }
    }
}