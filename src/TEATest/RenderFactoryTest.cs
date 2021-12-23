using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TEA;
using TestTools;

namespace TEATest {

    public class RenderFactoryTest {

        [Test]
        public void ApplyToListTest() {
            var destList = new List<int>();

            destList.ApplyToList(Enumerable.Range(0, 1)).Is(1);
            destList.ToArray().Is(new [] {0});

            // list is same
            destList.ApplyToList(Enumerable.Range(0, 1)).Is(1);
            destList.ToArray().Is(new [] {0});

            destList.ApplyToList(Enumerable.Range(0, 0)).Is(0);
            destList.ToArray().Is(new [] {0});

            destList.ApplyToList(Enumerable.Range(0, 2)).Is(2);
            destList.ToArray().Is(new [] {0, 1});
        }
    }
}
