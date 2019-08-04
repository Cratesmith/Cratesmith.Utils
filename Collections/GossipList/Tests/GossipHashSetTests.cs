using System;
using Cratesmith;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools.Constraints;

namespace Tests
{
    public class GossipHashSetTests
    {
        [Test]
        public void ExceptWith()
        {
            var setA = new GossipHashSet<int>(new int[]{0,1,2,3,4});
            var setB = new GossipHashSet<int>(new int[]{1,2,3,5});
            var setC = new GossipHashSet<int>(new int[]{0,4});

            // force call static constructors (ensure pools aren't made during gc check)
            new GossipPauseScope(null);
            new PreallocLinkList<object>();
            
            int count = 0;
            setA.OnChanged += list =>
            {
                ++count;
            };

            bool countFail = false;
            bool opFail = false;
            //Debug.Log("Entering GC test");
            //Assert.That(() =>
            { 
                setA.ExceptWith(setB);
                countFail |= setA.ChangeCount != 1;
                opFail |= !setA.SetEquals(setC);
            }
            //, Does.Not.AllocatingGCMemory());
            //Debug.Log("Exit GC test");

            Assert.IsFalse(countFail || setA.ChangeCount!=count);
            Assert.IsFalse(opFail);
        }

        [Test]
        public void IntersectWith()
        {
            var setA = new GossipHashSet<int>(new int[]{0,1,2,3,4});
            var setB = new GossipHashSet<int>(new int[]{1,2,3,5});
            var setC = new GossipHashSet<int>(new int[]{1,2,3});

            // force call static constructors (ensure pools aren't made during gc check)
            new GossipPauseScope(null);
            new PreallocLinkList<object>();
            
            int count = 0;
            setA.OnChanged += list =>
            {
                ++count;
            };

            bool countFail = false;
            bool opFail = false;
            //Debug.Log("Entering GC test");
            //Assert.That(() =>
            { 
                setA.IntersectWith(setB);
                countFail |= setA.ChangeCount != 1;
                opFail |= !setA.SetEquals(setC);
            }
            //, Does.Not.AllocatingGCMemory());
            //Debug.Log("Exit GC test");

            Assert.IsFalse(countFail || setA.ChangeCount!=count);
            Assert.IsFalse(opFail);
        }

        [Test]
        public void SymmetricExceptWith()
        {
            var setA = new GossipHashSet<int>(new int[]{0,1,2,3,4});
            var setB = new GossipHashSet<int>(new int[]{1,2,3,5});
            var setC = new GossipHashSet<int>(new int[]{0,4,5});

            // force call static constructors (ensure pools aren't made during gc check)
            new GossipPauseScope(null);
            new PreallocLinkList<object>();
            
            int count = 0;
            setA.OnChanged += list =>
            {
                ++count;
            };

            //Debug.Log("Entering GC test");
            bool countFail = false;
            bool opFail = false;
            //Assert.That(() =>
            { 
                setA.SymmetricExceptWith(setB);
                countFail |= setA.ChangeCount != 1;
                opFail |= !setA.SetEquals(setC);
            }
            //, Does.Not.AllocatingGCMemory());
            //Debug.Log("Exit GC test");

            Assert.IsFalse(countFail || setA.ChangeCount!=count);
            Assert.IsFalse(opFail);
        }

        [Test]
        public void UnionWith()
        {
            var setA = new GossipHashSet<int>(new int[]{0,1,2});
            var setB = new GossipHashSet<int>(new int[]{3,4,5});
            var setC = new GossipHashSet<int>(new int[]{0,1,2,3,4,5});

            // force call static constructors (ensure pools aren't made during gc check)
            new GossipPauseScope(null);
            new PreallocLinkList<object>();
            
            int count = 0;
            setA.OnChanged += list =>
            {
                ++count;
            };

            //Debug.Log("Entering GC test");
            bool countFail = false;
            bool opFail = false;
            //Assert.That(() =>
            { 
                setA.UnionWith(setB);
                countFail |= setA.ChangeCount != 1;
                opFail |= !setA.SetEquals(setC);
            }
            //, Does.Not.AllocatingGCMemory());
            //Debug.Log("Exit GC test");

            Assert.IsFalse(countFail || setA.ChangeCount!=count);
            Assert.IsFalse(opFail);
        }


        [Test]
        public void Add()
        {
            var gossipList = new GossipHashSet<int>();

            // force call static constructors (ensure pools aren't made during gc check)
            new GossipPauseScope(null);
            new PreallocLinkList<object>();
            
            int count = 0;
            gossipList.OnChanged += list =>
            {
                ++count;
            };

            bool countFail = false;
            //Debug.Log("Entering GC test");
            //Assert.That(() =>
            { 
                gossipList.Add(0);
                countFail |= gossipList.ChangeCount != 1;
                using (new GossipPauseScope(gossipList))
                {
                    gossipList.Add(1);
                    gossipList.Add(2);
                    gossipList.Add(3);
                }
                countFail |= gossipList.ChangeCount != 2;
                gossipList.Add(4);
                countFail |= gossipList.ChangeCount != 3;
            }
            //, Does.Not.AllocatingGCMemory());
            //Debug.Log("Exit GC test");

            Assert.IsFalse(countFail);
        }

        [Test]
        public void Remove()
        {
            var gossipList = new GossipHashSet<int>(new []{0,1,2,3,4});

            // force call static constructors (ensure pools aren't made during gc check)
            new GossipPauseScope(null);
            new PreallocLinkList<object>();
            
            int count = 0;
            gossipList.OnChanged += list =>
            {
                ++count;
            };

            bool countFail = false;
            bool opFail = false;
            //Debug.Log("Entering GC test");
            //Assert.That(() =>
            { 
                gossipList.Remove(0);
                opFail |= gossipList.Contains(0) || gossipList.Count != 4;
                countFail |= gossipList.ChangeCount != 1;
                using (new GossipPauseScope(gossipList))
                {
                    gossipList.Remove(1);
                    opFail |= gossipList.Contains(1) || gossipList.Count != 3;
                    gossipList.Remove(2);
                    opFail |= gossipList.Contains(2) || gossipList.Count != 2;
                    gossipList.Remove(3);
                    opFail |= gossipList.Contains(3) || gossipList.Count != 1;
                }
                countFail |= gossipList.ChangeCount != 2;
                gossipList.Remove(4);
                opFail |= gossipList.Contains(4) || gossipList.Count != 0;
                countFail |= gossipList.ChangeCount != 3;
            }
            //, Does.Not.AllocatingGCMemory());
            //Debug.Log("Exit GC test");

            Assert.IsFalse(countFail || gossipList.ChangeCount!=count);
            Assert.IsFalse(opFail);
        }

        [Test]
        public void Clear()
        {
            var gossipList = new GossipHashSet<int>(new []{0,1,2,3,4});

            // force call static constructors (ensure pools aren't made during gc check)
            new GossipPauseScope(null);
            new PreallocLinkList<object>();
            
            int count = 0;
            gossipList.OnChanged += list =>
            {
                ++count;
            };

            Debug.Log("Entering GC test");
            bool countFail = false;
            bool opFail = false;
            Assert.That(() =>
            { 
                gossipList.Clear();
                opFail |= gossipList.Count != 0;
                countFail |= gossipList.ChangeCount != 1;
            }, Does.Not.AllocatingGCMemory());
            Debug.Log("Exit GC test");

            Assert.IsFalse(countFail || gossipList.ChangeCount!=count);
            Assert.IsFalse(opFail);
        }

        
        [Test]
        public void RemoveWhere()
        {
            var gossipList = new GossipHashSet<int>(new []{0,1,3});

            // force call static constructors (ensure pools aren't made during gc check)
            new GossipPauseScope(null);
            new PreallocLinkList<object>();
            var action = new Predicate<int>(x => x < 3);
            
            int count = 0;
            gossipList.OnChanged += list =>
            {
                ++count;
            };

            Debug.Log("Entering GC test");
            bool countFail = false;
            bool opFail = false;
            Assert.That(() =>
            { 
                gossipList.RemoveWhere(action);
                opFail |= gossipList.Count != 1;
                countFail |= gossipList.ChangeCount != 1;
            }, Does.Not.AllocatingGCMemory());
            Debug.Log("Exit GC test");

            Assert.IsFalse(countFail || gossipList.ChangeCount!=count);
            Assert.IsFalse(opFail); 
        }
    }
}
