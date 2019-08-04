using System;
using Cratesmith;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools.Constraints;

namespace Tests
{
    public class GossipListTests
    {
        [Test]
        public void Add()
        {
            var gossipList = new GossipList<int>(100);

            // force call static constructors (ensure pools aren't made during gc check)
            new GossipPauseScope(null);
            new PreallocLinkList<object>();

            var count = 0;
            gossipList.OnChanged += list => { ++count; };

            var countFail = false;
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
            var gossipList = new GossipList<int>(new[] {0, 1, 2, 3, 4});

            // force call static constructors (ensure pools aren't made during gc check)
            new GossipPauseScope(null);
            new PreallocLinkList<object>();

            var count = 0;
            gossipList.OnChanged += list => { ++count; };

            var countFail = false;
            var opFail = false;
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

            Assert.IsFalse(countFail || gossipList.ChangeCount != count);
            Assert.IsFalse(opFail);
        }

        [Test]
        public void Clear()
        {
            var gossipList = new GossipList<int>(new[] {0, 1, 2, 3, 4});

            // force call static constructors (ensure pools aren't made during gc check)
            new GossipPauseScope(null);
            new PreallocLinkList<object>();

            var count = 0;
            gossipList.OnChanged += list => { ++count; };

            Debug.Log("Entering GC test");
            var countFail = false;
            var opFail = false;
            Assert.That(() =>
            {
                gossipList.Clear();
                opFail |= gossipList.Count != 0;
                countFail |= gossipList.ChangeCount != 1;
            }, Does.Not.AllocatingGCMemory());
            Debug.Log("Exit GC test");

            Assert.IsFalse(countFail || gossipList.ChangeCount != count);
            Assert.IsFalse(opFail);
        }


        [Test]
        public void RemoveAll()
        {
            var gossipList = new GossipList<int>(new[] {0, 1, 0, 3, 0});

            // force call static constructors (ensure pools aren't made during gc check)
            new GossipPauseScope(null);
            new PreallocLinkList<object>();
            var action = new Predicate<int>(x => x == 0);

            var count = 0;
            gossipList.OnChanged += list => { ++count; };

            Debug.Log("Entering GC test");
            var countFail = false;
            var opFail = false;
            Assert.That(() =>
            {
                gossipList.RemoveAll(action);
                opFail |= gossipList.Count != 2;
                countFail |= gossipList.ChangeCount != 1;
            }, Does.Not.AllocatingGCMemory());
            Debug.Log("Exit GC test");

            Assert.IsFalse(countFail || gossipList.ChangeCount != count);
            Assert.IsFalse(opFail);
        }


        [Test]
        public void Sort()
        {
            var unsortedList = new GossipList<int>(new[] {4, 1, 2, 0, 3});
            var sortedList = new GossipList<int>(new[] {0, 1, 2, 3, 4});

            unsortedList.Sort();
            sortedList.Sort();

            Assert.AreEqual(unsortedList.ChangeCount, 1);
            Assert.AreEqual(sortedList.ChangeCount, 0);

            for (var i = 0; i < unsortedList.Count; i++) Assert.AreEqual(unsortedList[i], sortedList[i]);
        }
    }
}