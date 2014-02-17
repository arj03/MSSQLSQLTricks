MSSQLSQLTricks
==============

Random collection of MSSQL tricks. Currently contains two things:

* An in memory database that can be used as a simple replacement for a data context for testing
* Extension methods for a LINQ data context that adds Bulk insert and Truncate / Bulk insert. This can in my experience quickly be an order of magnitude faster than normal InsertOnSubmit.
