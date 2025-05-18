# MondoCore.Data

<br>

A collection of interfaces and classes for accessing data from databases and other data sources


## Table of Contents
- [Interfaces](#interfaces)
    - [IReadRepository](#readrepo)
    - [IWriteRepository](#writerepo)
    - [IDatabase](#database)
    - [IIdentifiable](#identifiable)
    - [IIdentifierStrategy](#identifier)
    - [IPartitionable](#partitionable)
    - [IPartitionedId](#ipartitionedid)
- [Classes](#classes)
    - [CachedRepository](#cachedrepo)
    - [DelimitedIdentifierStrategy](#delimitedidentifierstrategy)
    - [FixedIdentifierStrategy](#fixedidentifierstrategy)
    - [PartitionedId](#partitionedid)
    - [NotFoundException](#notfound)
- [Extensions](#extensions)
    - [ToPartitionedId](#topartitionedid)

<br/>

<a name="interfaces"></a>
## Interfaces

***
<a name="readrepo"></a>
### IReadRepository
> Read data from a repository (data source)

<a name="readrepo"></a>
#### Task\<TValue\> Get(TID id)
> Retrieve a single object with the given id

<a name="readrepo"></a>
#### IAsyncEnumerable\<TValue\> Get(IEnumerable\<TID\> ids)
> Retrieve a list of objects with the given ids

<a name="readrepo"></a>
#### IAsyncEnumerable\<TValue\> Get(Expression<Func<TValue, bool>> query)
> Query for a list of objects that match the given expression


    public IAsyncEnumerable<Customer> GetCustomers(string city)
    {
        return _reader.Get( (c)=> c.City == city);
    }

***
<a name="writerepo"></a>
### IWriteRepository
> Write data to a repository (data source)

#### Task<TValue> Insert(TValue item)
> Insert a new object into the repository

#### Task Insert(IEnumerable<TValue> items)
> Insert a list of objects into the repository

#### Task\<bool\> Update(TValue item, Expression<Func<TValue, bool>> guard = null)
> Update a single object with a guard

    _writer.Update(customer, (c)=> c.Status == "active");

#### Task\<long\> Update(object properties, Expression<Func<TValue, bool>> query);       
> Retrieve a list of objects that match the given query and update the given properties. Returns the number of items updated.

    // Change city of all the items with city of "Lower Junction" to "Springfield"
    _writer.Update( new { City = "Springfield" }, (item)=> item.City == "Lower Junction");

#### Task\<long\> Update(Func<TValue, Task<(bool Update, bool Continue)>> update, Expression<Func<TValue, bool>> query);       
> Retrieve a list of objects that match the given query and updates using the given lambda expression to modify properties. Returns the number of items updated.

    // Change city of all the items with city of "Lower Junction" to "Springfield"
    _writer.Update( (i)=>
    {
        i.City = "Springfield",

        return Task.FromResult((true, true));
    },
    new { City = "Springfield" }, (item)=> item.City == "Lower Junction");

<a name="writerepo"></a>
#### Task\<bool\> Delete(TID id)
> Delete a single object

<a name="writerepo"></a>
#### Task\<long\> Delete(Expression<Func<TValue, bool>> guard)
> Delete a list of objects that match the given query. Returns the number of items deleted.

    _writer.Delete( (item)=> item.Status == "Archived");


***
<a name="database"></a>
### IDatabase
> Retrieve read and write repository interfaces from a database. A repository may be a sql database table or a collection in a NoSql database.

#### IReadRepository\<TID, TValue\> GetRepositoryReader\<TID, TValue\>(string repoName, IIdentifierStrategy\<TID\> strategy = null) where TValue : IIdentifiable\<TID\>
> Retrieve a reader for a repository with the given name and identifier strategy

#### IWriteRepository\<TID, TValue\> GetRepositoryWriter\<TID, TValue\>(string repoName, IIdentifierStrategy\<TID\> strategy = null) where TValue : IIdentifiable\<TID\>
> Retrieve a writer for a repository with the given name and identifier strategy

#### (extension) IReadRepository<TID, TValue> GetRepositoryReader<TID, TValue>(string repoName, string partitionKey) where TValue : IIdentifiable<TID>
> Retrieve a reader for a repository with the given name using a fixed identifier strategy

#### (extension) IWriteRepository<TID, TValue> GetRepositoryWriter<TID, TValue>(string repoName, string partitionKey) where TValue : IIdentifiable<TID>
> Retrieve a writer for a repository with the given name using a fixed identifier strategy

***
<a name="identifiable"></a>
### IIdentifiable\<TID\>
> The value type for a respository must derive from the interface.

#### TID Id 

***
<a name="identifier"></a>
### IIdentifierStrategy\<TID\>
> The read and write interfaces take a single identifier but certain kinds of repositories/databases might also take a partition key. This interface specifies how to extract the partition key.

***
<a name="partitionable"></a>
### IPartitionable\<TID\>
> If a repository is partitioned then the value type for a respository must derive from the interface.

#### string GetPartitionKey()


<a name="ipartitionedid"></a>
### IPartitionedId
> Represents an object with a repository item id and a partition key

#### string Id           
#### string PartitionKey 

<br/>

<a name="classes"></a>
## Classes 

***
<a name="cachedrepo"></a>
### CachedRepository\<TID, TValue\> : IReadRepository\<TID, TValue\>
> Provides a way to cache the contents of a repository (as needed) using another repository, usually a memory only repository.

***
<a name="partitionedid"></a>
### PartitionedId : IPartitionedId
> A model class that contains an identifier and a partiton key.

#### string Id           
#### string PartitionKey 

***
<a name="delimitedidentifierstrategy"></a>
### DelimitedIdentifierStrategy\<TID\> : IIdentifierStrategy\<TID\>
> Represents a strategy where the key passed into a repository reader/writer is a delimited id and partition key, e.g. "id;partitionkey"

***
<a name="fixedidentifierstrategy"></a>
### FixedIdentifierStrategy\<TID\> : IIdentifierStrategy\<TID\>
> Represents a strategy where the key passed into a repository reader/writer is a fixed partition key and the key is the identifier alone.

***
<a name="notfound"></a>
### NotFoundException
> An exception to indicate when an object was not found

<br>

<a name="extensions"></a>
## Extensions 

<br>

