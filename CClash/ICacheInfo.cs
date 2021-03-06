﻿using System;
namespace CClash
{
    public interface ICacheInfo : IDisposable
    {
        long CacheHits { get; set; }
        long CacheMisses { get; set; }
        long CacheObjects { get; set; }
        long CacheSize { get; set; }
        long CacheUnsupported { get; set; }
        void LockStatsCall(Action x);
        long SlowHitCount { get; set; }
        void Commit();
    }
}
