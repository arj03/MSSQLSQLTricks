using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Linq;

namespace SQLTricks
{
    public class InMemoryTable<T> : List<T> where T : class
    {
        public InMemoryTable(InMemoryDataContext context)
        {
            context.startTransactions.Add(this.StartTransaction);
            context.endTransactions.Add(this.EndTransaction);
            context.commitTransactions.Add(this.Commit);
            context.rollbackTransactions.Add(this.Rollback);
        }

        // mimic linq
        Stack<List<T>> uncomittedChanges = new Stack<List<T>>();

        int uniqueId = 0;

        public void InsertOnSubmit(T item)
        {
            var wrapped = FastMember.ObjectAccessor.Create(item);
            wrapped["Id"] = uniqueId++;
            this.Add(item);
            uncomittedChanges.Peek().Add(item);
        }

        public void DeleteAllOnSubmit(EntitySet<T> items)
        {
            // double reference, so we delete both ends

            // FIXME: should use transaction
            foreach (var item in items)
                this.Remove(item);

            items.Clear();
        }

        public void DeleteAllOnSubmit(IEnumerable<T> items)
        {
            // FIXME: should use transaction
            foreach (var item in items)
                this.Remove(item);
        }

        public void DeleteOnSubmit(T item)
        {
            // FIXME: should use transaction
            this.Remove(item);
        }

        public void StartTransaction()
        {
            uncomittedChanges.Push(new List<T>());
        }

        public void EndTransaction()
        {
            uncomittedChanges.Pop();
        }

        public void Commit()
        {
            uncomittedChanges.Peek().Clear();
        }

        public void Rollback()
        {
            var latestTransaction = uncomittedChanges.Peek();
            foreach (var c in latestTransaction)
                this.Remove(c);
        }
    }

    // this class can be used to simulate a LINQ db data context using simple in memory lists
    public class InMemoryDataContext : IDisposable
    {
        // declare your tables here:
        //public InMemoryTable<Customer> Customers;

        public List<Action> startTransactions = new List<Action>();
        public List<Action> commitTransactions = new List<Action>();
        public List<Action> rollbackTransactions = new List<Action>();
        public List<Action> endTransactions = new List<Action>();

        InMemoryDataContext()
        {
            //Customers = new InMemoryTable<Customer>(this);
        }

        static InMemoryDataContext instance;

        List<object> tables = new List<object>();

        public static InMemoryDataContext GetInstance()
        {
            if (instance == null)
            {
                instance = new InMemoryDataContext();
            }

            foreach (var f in instance.startTransactions)
                f();

            return instance;
        }

        public void Cleanup()
        {
            instance = null;
        }

        public void SubmitChanges()
        {
            foreach (var f in instance.commitTransactions)
                f();
        }

        public void Rollback()
        {
            foreach (var f in instance.rollbackTransactions)
                f();
        }

        public void Dispose()
        {
            Rollback();

            foreach (var f in instance.endTransactions)
                f();
        }
    }
}
