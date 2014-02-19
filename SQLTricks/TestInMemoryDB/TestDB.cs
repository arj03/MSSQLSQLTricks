using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SQLTricks;

namespace TestInMemoryDB
{
    [TestClass]
    public class TestDB
    {
        InMemoryDataContext db;

        [TestInitialize()]
        public void MyTestInitialize()
        {
            db = InMemoryDataContext.GetInstance();
        }

        [TestCleanup()]
        public void MyTestCleanup()
        {
            db.Dispose();
            db.Cleanup();
        }

        [TestMethod]
        public void TestInsert()
        {
            Assert.AreEqual(db.Customers.Count(), 0, "Db is empty");
            
            var t = new Customer()
            {
                Email = "test@test.dk"
            };

            db.Customers.InsertOnSubmit(t);

            Assert.AreEqual(db.Customers.Count(), 0, "Uncomitted changes are not visible");

            db.SubmitChanges();

            Assert.AreEqual(db.Customers.Count(), 1, "Customer now in database");
        }

        [TestMethod]
        public void TestDelete()
        {
            Assert.AreEqual(db.Customers.Count(), 0, "Db is empty");

            var t = new Customer()
            {
                Email = "test@test.dk"
            };

            db.Customers.InsertOnSubmit(t);
            db.SubmitChanges();

            Assert.AreEqual(db.Customers.Count(), 1, "Customer now in database");

            db.Customers.DeleteOnSubmit(db.Customers.First());

            Assert.AreEqual(db.Customers.Count(), 1, "Uncomitted changes are not visible");

            db.SubmitChanges();

            Assert.AreEqual(db.Customers.Count(), 0, "Customer now deleted in database");
        }
    }
}
