using System;

namespace McGuireV10.OrleansDistributedCache
{
    /// <summary>
    /// Client configuration options. Use this with the AddOrleansDistributedCache extension method.
    /// </summary>
    public class OrleansDistributedCacheOptions
    {
        public OrleansDistributedCacheOptions() 
        { }

        /// <summary>
        /// Controls whether the cached value is persisted to storage immediately when set, or whether this is
        /// deferred until Orleans decides to deactivate the item. The default is true.
        /// </summary>
        public bool PersistWhenSet { get; set; } = true;

        /// <summary>
        /// The minimum amount of time before Orleans can deactivate the cached value. Deactivation is when the
        /// cached value is persisted to storage and unloaded from memory. The next access will involve a delay
        /// while a new copy is loaded into memory. The default is 5 minutes.
        /// </summary>
        public TimeSpan DefaultDelayDeactivation { get; set; } = TimeSpan.FromMinutes(5);
    }
}
