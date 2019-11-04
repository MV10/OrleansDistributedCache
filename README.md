
# Orleans Distributed Cache

NuGet: [McGuireV10.OrleansDistributedCache](https://www.nuget.org/packages/McGuireV10.OrleansDistributedCache/)

This is a .NET Standard 2.0 implementation of the [`IDistributedCache`](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.caching.distributed.idistributedcache?view=dotnet-plat-ext-3.0) mechanism built on top of [Microsoft Orleans](http://dotnet.github.io/orleans/), a high-scale virtual actor framework. You must provide the Orleans Silo (host). This package only provides the Grain (actor), the tie-in to the cache interface, and some convenience features such as the `AddOrleansDistributedCache` dependency injection helper extension.

## Requirements

* Microsoft Orleans 3.0 Silo
* An Orleans Persistence (grain storage) data store named with this constant:
* `OrleansDistributedCacheService.OrleansDistributedCacheStorageProviderName`

See the articles linked at the bottom of this readme and the repo sample code for information.

## Usage

This is a standard .NET interface, please refer to the Microsoft [documentation](https://docs.microsoft.com/en-us/aspnet/core/performance/caching/distributed?view=aspnetcore-3.0) for distributed caching.

We **strongly** recommend against the use of this package for stateful `ISession` storage in web-based apps.

Session state is widely considered an anti-pattern for web-based applications, but it is particularly bad with an Orleans-based cache. The Orleans part of this system persists cache entries by key, and session storage caches everything using the session ID as the key. Even if the cache entry is cleared (which ASP.NET does not guarantee since there is no standard mechanism for a browser to signal shutdown back to the server), "clearing" the state of an Orleans grain only persists a null value for that key (for performance reasons). This means you will have one row in your database for _every_ session that has _ever_ existed, unless you add a separate cleanup process (probably based on activity date, which Orleans also stores in the table).

ASP.NET itself doesn't need session storage. Just use the distribted cache directly -- produce unique keys based on elements like the user ID (which might be an email address, or an Identity Provider Subject ID). 

## Articles

This is the repository for the NuGet package McGuireV10.OrleansDistributedCache, and also for code from my 2019 articles describing the project and related demo projects. Currently the repository is not set up for automated builds and distributions, I update NuGet manually if/when necessary.

### [Distributed Caching with Microsoft Orleans](https://mcguirev10.com/2019/09/18/distributed-caching-with-microsoft-orleans.html)

### [Orleans Distributed Cache NuGet Release](https://mcguirev10.com/2019/11/04/orleans-distributed-cache-nuget-release.html)