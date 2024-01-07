using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Assertions;

namespace Assets.Scripts.Tests
{
    public class StructListTests
    {
        struct TestStruct
        {
            public int x;
            public int y;
        }
        public static void Test()
        {
            StructList<TestStruct> tlist = new StructList<TestStruct>();
            TestStruct[] els = new TestStruct[20];
            for(int i = 0; i < els.Length; i++)
            {
                els[i].x = i;
                els[i].y = 20*i;
                tlist.Add(ref els[i]);
            }
            Assert.IsTrue(els.Length == tlist.Count, "Count did not match expected!");
            Assert.IsTrue(els.Length == tlist.Array.Length, "Internal array size did not match expected!");
            for (int i = 0; i < tlist.Count; i++)
            {
                Assert.IsTrue(tlist.Array[i].x == i);
                Assert.IsTrue(tlist.Array[i].y == 20*i);
            }
            int countBefore = tlist.Count;
            tlist.RemoveAt(0);
            Assert.IsTrue(countBefore - 1 == tlist.Count);
            Assert.IsTrue(tlist.Array[0].x == 1 && tlist.Array[0].y == 20*1);

            countBefore = tlist.Count;
            tlist.RemoveAt(tlist.Count-1);
            Assert.IsTrue(countBefore - 1 == tlist.Count);
            Assert.IsTrue(tlist.Array[0].x == 1 && tlist.Array[0].y == 20 * 1);

            countBefore = tlist.Count;
            tlist.RemoveAll(x => x.x == 5 || x.x == 6);
            Assert.IsTrue(countBefore - 2 == tlist.Count);
            Assert.IsTrue(tlist.Array[0].x == 1 && tlist.Array[0].y == 20 * 1);
            Assert.IsTrue(tlist.Array[tlist.Count-1].x == 18 && tlist.Array[tlist.Count - 1].y == 20 * 18);
        }
    }
}
