using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Linq;

namespace SQLTricks
{
    public class InMemoryIdentityTable<T> : InMemoryTable<T> where T : class
    {
        public InMemoryIdentityTable(InMemoryDataContext context, string identityAttribute)
            : base(context)
        {
            this.identityAttribute = identityAttribute;
        }

        public InMemoryIdentityTable(InMemoryDataContext context)
            : base(context)
        {
            this.identityAttribute = "Id"; // default
        }

        string identityAttribute = "";
        int uniqueId = 0;

        public override void InsertOnSubmit(T item)
        {
            if (identityAttribute != "")
            {
                var wrapped = FastMember.ObjectAccessor.Create(item);
                wrapped[identityAttribute] = uniqueId++;
            }

            uncomittedInserts.Peek().Add(item);
        }
    }

    public class InMemoryTable<T> : List<T> where T : class
    {
        public InMemoryTable(InMemoryDataContext context)
        {
            context.startTransactions.Add(this.StartTransaction);
            context.endTransactions.Add(this.EndTransaction);
            context.commitTransactions.Add(this.Commit);
        }

        // mimic linq
        protected Stack<List<T>> uncomittedInserts = new Stack<List<T>>();
        protected Stack<List<T>> uncomittedDeletes = new Stack<List<T>>();

        public virtual void InsertOnSubmit(T item)
        {
            uncomittedInserts.Peek().Add(item);
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
            var topTransaction = uncomittedDeletes.Peek();

            foreach (var item in items)
                topTransaction.Add(item);
        }

        public void DeleteOnSubmit(T item)
        {
            uncomittedDeletes.Peek().Add(item);
        }

        public void StartTransaction()
        {
            uncomittedInserts.Push(new List<T>());
            uncomittedDeletes.Push(new List<T>());
        }

        public void EndTransaction()
        {
            uncomittedInserts.Pop();
            uncomittedDeletes.Pop();
        }

        public void Commit()
        {
            foreach (var insert in uncomittedInserts.Peek())
                this.Add(insert);

            uncomittedInserts.Peek().Clear();

            foreach (var delete in uncomittedDeletes.Peek())
                this.Remove(delete);

            uncomittedDeletes.Peek().Clear();
        }
    }

    // this class can be used to simulate a LINQ db data context using simple in memory lists
    public class InMemoryDataContext : IDisposable
    {
        // declare your tables here:
        public InMemoryIdentityTable<Customer> Customers;

        public List<Action> startTransactions = new List<Action>();
        public List<Action> commitTransactions = new List<Action>();
        public List<Action> endTransactions = new List<Action>();

        InMemoryDataContext()
        {
            Customers = new InMemoryIdentityTable<Customer>(this);
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

        public void Dispose()
        {
            foreach (var f in instance.endTransactions)
                f();
        }
    }
}
