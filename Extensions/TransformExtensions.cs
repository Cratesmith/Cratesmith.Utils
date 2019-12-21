using Cratesmith.Collections.Temp;
using UnityEngine;

namespace Cratesmith.Utils
{
    public static class TransformExtensions 
    {
        public static void Reset(this Transform @this)
        {
            @this.localPosition = Vector3.zero;
            @this.localRotation = Quaternion.identity;
            @this.localScale = Vector3.one;
        }
	
        public static Transform FindChildDepthFirst(this Transform @this, System.Func<Transform,bool> test)
        {
            if (test.Invoke(@this))
                return @this;

            foreach(Transform child in @this)
            {
                var result = child.FindChildDepthFirst(test);
                if (result != null)
                    return result;
            }
            return null;
        }

        public static Transform FindChildBreadthFirst(this Transform @this, System.Func<Transform,bool> test)
        {
            foreach (Transform child in @this)
            {
                if (test.Invoke(child))
                    return child;
            }

            foreach(Transform child in @this)
            {
                var result = child.FindChildBreadthFirst(test);
                if (result != null)
                    return result;
            }
            return null;
        }

        public static Bounds GetLocalMeshRenderBounds(this Transform @this, int layerMask= -1)
        {
            Bounds output = new Bounds(Vector3.zero, Vector3.zero);
            using (var vecList = TempList<Vector3>.Get())
            using (var list = @this.GetComponentsInChildrenTempList<Renderer>())
                foreach (var renderer in list)
                {
                    var bounds = new Bounds(Vector3.zero, Vector3.zero);
                    if (!(renderer is SkinnedMeshRenderer) && !(renderer is MeshRenderer))
                    {
                        continue;
                    }

                    if ((1 << renderer.gameObject.layer & layerMask)==0)
                    {
                        continue;
                    }

                    if (renderer.enabled == false)
                    {
                        continue;
                    }

                    var smr = renderer as SkinnedMeshRenderer;
                    if (smr)
                    {
                        smr.localBounds.GetCorners(vecList);
                        for (int i = 0; i < vecList.Count; i++)
                        {
                            vecList[i] = smr.transform.TransformPoint(vecList[i]);
                        }
                    }
                    else
                    {
                        renderer.bounds.GetCorners(vecList);	
                    }
			
                    foreach (var corner in vecList)
                    {
                        bounds.Encapsulate(@this.transform.InverseTransformPoint(corner));
                    }			

                    output.Encapsulate(bounds);			
                }
            return output;
        }

        public static Bounds TransformBounds(this Transform @this, Bounds bounds)
        {
            using (var list = TempList<Vector3>.Get())
            {
                bounds.GetCorners(list);
                for (int i = 0; i < list.Count; i++)
                {
                    list[i] = @this.TransformPoint(list[i]);
                }

                Bounds output = new Bounds(list[0],Vector3.zero);
                for (int i = 1; i < list.Count; i++)
                {
                    output.Encapsulate(list[i]);
                }
                return output;
            }
        }
    }
}
