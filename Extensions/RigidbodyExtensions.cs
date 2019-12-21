using System.Collections.Generic;
using Cratesmith.Collections.Temp;
using UnityEngine;

namespace Cratesmith.Utils
{
    public static class RigidbodyExtensions
    {
        private static HashSet<Collider> s_colliderSet = new HashSet<Collider>();

        private static Dictionary<Collider, HashSet<Collider>> s_ignores = new Dictionary<Collider, HashSet<Collider>>();

        public static void IgnoreCollisionsWith(this Rigidbody @this, Collider otherCollider, bool ignore = true)
        {
            if (otherCollider == null)
            {
                return;
            }

            bool cleanup = false;
            using (var list = @this.GetComponentsInChildrenTempList<Collider>())
            {
                foreach (var myCollider in list)
                {				
                    RegisterIgnore(myCollider, otherCollider, ignore);
                    Physics.IgnoreCollision(myCollider, otherCollider, ignore);
                }
            }

            if(cleanup) CleanupTables();
        }

        private static void CleanupTables()
        {
            foreach (var collider in s_colliderSet)
            {
                if (collider)
                {
                    continue;
                }

                foreach (var table in s_ignores)
                {
                    table.Value.Remove(collider);
                }

                s_colliderSet.Remove(collider);
            }
        }

        private static void RegisterIgnore(Collider myCollider, Collider otherCollider, bool ignore)
        {
            HashSet<Collider> tableA = null;
            if (!s_ignores.TryGetValue(myCollider, out tableA) && ignore)
            {
                tableA = s_ignores[myCollider] = new HashSet<Collider>();
            }

            HashSet<Collider> tableB = null;
            if (!s_ignores.TryGetValue(myCollider, out tableB) && ignore)
            {
                tableB = s_ignores[myCollider] = new HashSet<Collider>();
            }

            if (ignore)
            {
                tableA.Add(otherCollider);
                tableB.Add(myCollider);
            }
            else
            {
                if(tableA!=null) tableA.Remove(otherCollider);
                if(tableB!=null) tableB.Remove(myCollider);
            }
        }

        public static void CopyIgnoredCollisionFrom(this Rigidbody @this, Rigidbody other)
        {      
            if (other == null)
            {
                return;
            }

            ClearIgnoredCollisions(@this);

            bool cleanup = false;
            using(var myList = @this.GetComponentsInChildrenTempList<Collider>())
            using(var otherList = other.GetComponentsInChildrenTempList<Collider>())
            {
                s_colliderSet.Clear();
                foreach (var collider in otherList)
                {
                    HashSet<Collider> table = null;
                    if (s_ignores.TryGetValue(collider, out table))
                    {
                        s_colliderSet.UnionWith(table);
                    }
                }

                foreach (var colliderA in myList)
                {
                    foreach (var colliderB in s_colliderSet)
                    {
                        if (!colliderB)
                        {
                            cleanup = true;
                            continue;
                        }
                        Physics.IgnoreCollision(colliderA, colliderB);
                    }            
                }
            }

            if(cleanup) CleanupTables();
        }

        public static void ClearIgnoredCollisions(this Rigidbody @this)
        {
            bool cleanup = false;
            using(var myList = @this.GetComponentsInChildrenTempList<Collider>())            
                foreach (var colliderA in myList)
                {
                    HashSet<Collider> table = null;
                    if (!s_ignores.TryGetValue(colliderA, out table))
                    {
                        continue;
                    }

                    foreach (var colliderB in table)
                    {
                        if (!colliderB)
                        {
                            cleanup = true;
                            continue;
                        }
		        
                        Physics.IgnoreCollision(colliderA, colliderB, false);	            
                    }
                }
	    
            if(cleanup) CleanupTables();
        }

        public static void IgnoreCollisionsWith(this Rigidbody @this, Rigidbody other, bool ignore = true, bool applyToTriggers=true)
        {
            if (other == null)
            {
                return;
            }

            using(var myList = @this.GetComponentsInChildrenTempList<Collider>())
            using(var otherList = other.GetComponentsInChildrenTempList<Collider>())
                foreach (var colliderA in myList)
                {
                    if (colliderA.isTrigger&&!applyToTriggers)
                    {
                        continue;
                    }

                    foreach (var colliderB in otherList)
                    {
                        if (colliderB.isTrigger&&!applyToTriggers)
                        {
                            continue;
                        }
                        RegisterIgnore(colliderA, colliderB, ignore);
                        Physics.IgnoreCollision(colliderA, colliderB, ignore);
                    }                
                }
        }

        public static void ClearVelocityAndIgnoredCollisions(this Rigidbody @this)
        {
            @this.velocity          = Vector3.zero;
            @this.angularVelocity   = Vector3.zero;
            ClearIgnoredCollisions(@this);
        }
    }
}
