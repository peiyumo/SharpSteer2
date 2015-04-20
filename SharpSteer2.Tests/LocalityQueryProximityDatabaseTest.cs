﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using SharpSteer2.Database;

namespace SharpSteer2.Tests
{
    [TestClass]
    public class LocalityQueryProximityDatabaseTest
    {
        [TestMethod]
        public void Construct()
        {
            var db = new LocalityQueryProximityDatabase<object>(Vector3.Zero, new Vector3(10, 10, 10), new Vector3(2, 2, 2));

            Assert.AreEqual(0, db.Count);
        }

        [TestMethod]
        public void AllocateToken()
        {
            var db = new LocalityQueryProximityDatabase<object>(Vector3.Zero, new Vector3(10, 10, 10), new Vector3(2, 2, 2));

            var obj = new object();
            var token = db.AllocateToken(obj);
            token.UpdateForNewPosition(Vector3.Zero);

            Assert.AreEqual(1, db.Count);

            token.UpdateForNewPosition(Vector3.Zero);
        }

        private static ITokenForProximityDatabase<object> CreateToken(IProximityDatabase<object> db, Vector3 position, IDictionary<object, Vector3> lookup)
        {
            var obj = new object();

            if (lookup != null)
                lookup.Add(obj, position);

            var token = db.AllocateToken(obj);
            token.UpdateForNewPosition(position);

            return token;
        }

        [TestMethod]
        public void LocateNeighbours()
        {
            var db = new LocalityQueryProximityDatabase<object>(Vector3.Zero, new Vector3(10, 10, 10), new Vector3(2, 2, 2));

            Dictionary<object, Vector3> positionLookup = new Dictionary<object, Vector3>();

            var xyz000 = CreateToken(db, new Vector3(0, 0, 0), positionLookup);
            var xyz100 = CreateToken(db, new Vector3(1, 0, 0), positionLookup);
            CreateToken(db, new Vector3(3, 0, 0), positionLookup);

            var list = new List<object>();
            xyz000.FindNeighbors(Vector3.Zero, 2, list);

            Assert.AreEqual(2, list.Count);
            Assert.AreEqual(1, list.Count(a => positionLookup[a] == new Vector3(0, 0, 0)));
            Assert.AreEqual(1, list.Count(a => positionLookup[a] == new Vector3(1, 0, 0)));

            //Check tokens handle being disposed twice
            xyz000.Dispose();
            xyz000.Dispose();

            list.Clear();
            xyz100.FindNeighbors(Vector3.Zero, 1.5f, list);

            Assert.AreEqual(1, list.Count);
            Assert.AreEqual(new Vector3(1, 0, 0), positionLookup[list[0]]);
        }

        [TestMethod]
        public void LocateNeighboursOutsideSuperbrick()
        {
            var db = new LocalityQueryProximityDatabase<object>(Vector3.Zero, new Vector3(1, 1, 1), new Vector3(1, 1, 1));

            Dictionary<object, Vector3> positionLookup = new Dictionary<object, Vector3>();

            var token = CreateToken(db, new Vector3(0, 0, 0), positionLookup);
            CreateToken(db, new Vector3(3, 0, 0), positionLookup);
            CreateToken(db, new Vector3(4, 0, 0), positionLookup);

            var list = new List<object>();
            token.FindNeighbors(Vector3.Zero, 2, list);
            Assert.AreEqual(1, list.Count);

            list.Clear();
            token.FindNeighbors(new Vector3(3, 0, 0), 0.1f, list);
            Assert.AreEqual(1, list.Count);

            list.Clear();
            token.FindNeighbors(new Vector3(3, 0, 0),1.1f, list);
            Assert.AreEqual(2, list.Count);
        }

        [TestMethod]
        public void RemoveItem()
        {
            var db = new LocalityQueryProximityDatabase<object>(Vector3.Zero, new Vector3(100, 100, 100), new Vector3(1, 1, 1));

            var a = CreateToken(db, new Vector3(1, 0, 0), null);
            var b = CreateToken(db, new Vector3(2, 0, 0), null);

            Assert.AreEqual(2, db.Count);

            b.Dispose();

            Assert.AreEqual(1, db.Count);

            a.Dispose();

            Assert.AreEqual(0, db.Count);
        }
    }
}
