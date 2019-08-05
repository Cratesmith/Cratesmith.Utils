using NUnit.Framework;

namespace Cratesmith.Utils.GossipList.Tests
{
    public class GossipDictionaryTests
    {      
        [Test]
        public void Add()
        {
            int count = 0;
            var dictionary = new GossipDictionary<int,string>();
            dictionary.OnChanged += _ => ++count;

            dictionary[0] = "Hello";
            using (new GossipPauseScope(dictionary))
            {
                dictionary[1] = "World";
                dictionary[2] = "!";
            }
            Assert.AreEqual("Hello", dictionary[0]);
            Assert.AreEqual(count, dictionary.ChangeCount);
            Assert.AreEqual(2, dictionary.ChangeCount);
        }

        [Test]
        public void Remove()
        {
            var dictionary = new GossipDictionary<int,string>
            {
                {0,"Hello"},
                {1,"World"},
                {2,"!"}
            };

            var count = dictionary.ChangeCount;
            dictionary.OnChanged += _ => ++count;

            Assert.IsTrue(dictionary.Remove(0));
            Assert.IsTrue(4 == count && count == dictionary.ChangeCount);
            Assert.AreEqual(2,dictionary.Count);
            
            Assert.IsFalse(dictionary.Remove(0));
            Assert.IsTrue(4 == count && count == dictionary.ChangeCount);        
            Assert.AreEqual(2,dictionary.Count);
        }

        [Test]
        public void Clear()
        {
            var dictionary = new GossipDictionary<int,string>
            {
                {0,"Hello"},
                {1,"World"},
                {2,"!"}
            };

            var count = dictionary.ChangeCount;
            dictionary.OnChanged += _ => ++count;

            dictionary.Clear();
            Assert.IsTrue(4 == count && count == dictionary.ChangeCount);
            Assert.AreEqual(0,dictionary.Count);
        }

        [Test]
        public void ContainsKey()
        {
            var dictionary = new GossipDictionary<int,string>
            {
                {0,"Hello"},
                {1,"World"},
                {2,"!"}
            };

            var count = dictionary.ChangeCount;

            Assert.IsTrue(dictionary.ContainsKey(0));
            Assert.IsTrue(dictionary.ContainsKey(1));
            Assert.IsTrue(dictionary.ContainsKey(2));
            Assert.IsFalse(dictionary.ContainsKey(3));
            Assert.AreEqual(count, dictionary.ChangeCount);
        }

        [Test]
        public void ContainsValue()
        {
            var dictionary = new GossipDictionary<int,string>
            {
                {0,"Hello"},
                {1,"World"},
                {2,"!"}
            };

            var count = dictionary.ChangeCount;
            
            Assert.IsTrue(dictionary.ContainsValue("Hello"));
            Assert.IsTrue(dictionary.ContainsValue("World"));
            Assert.IsTrue(dictionary.ContainsValue("!"));
            Assert.IsFalse(dictionary.ContainsValue("Bacon"));
            Assert.AreEqual(count, dictionary.ChangeCount);
        }

        [Test]
        public void TryGetValue()
        {
            var dictionary = new GossipDictionary<int,string>
            {
                {0,"Hello"},
                {1,"World"},
                {2,"!"}
            };

            var count = dictionary.ChangeCount;

            Assert.IsTrue(dictionary.TryGetValue(0, out var value) && value == "Hello");
            Assert.IsTrue(dictionary.TryGetValue(1, out var value1) && value1 == "World");
            Assert.IsTrue(dictionary.TryGetValue(2, out var value2) && value2 == "!");
            Assert.IsFalse(dictionary.TryGetValue(3, out var __));
            Assert.AreEqual(count, dictionary.ChangeCount);
        }
    }
}