﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CClash
{
    public sealed class FastCacheInfo : IDisposable, ICacheInfo
    {
        FileCacheStore statstore;
        public FastCacheInfo(FileCacheStore stats)
        {
            statstore = stats;
        }

        public long CacheHits
        {
            get;
            set;
        }

        public long CacheMisses
        {
            get;
            set;
        }

        public long CacheObjects
        {
            get;
            set;
        }

        public long CacheSize
        {
            get;
            set;
        }

        public long CacheUnsupported
        {
            get;
            set;
        }

        public void LockStatsCall(Action x)
        {
            x.Invoke();
        }

        public long MSecLost
        {
            get;
            set;
        }

        public long MSecSaved
        {
            get;
            set;
        }


        public bool OmitLocks
        {
            get;
            set;
        }

        public long SlowHitCount
        {
            get;
            set;
        }

        public void Commit()
        {
            var real = new CacheInfo(statstore);
            real.LockStatsCall(() =>
            {
                real.MSecLost += this.MSecLost;
                real.SlowHitCount += this.SlowHitCount;
                real.CacheUnsupported += this.CacheUnsupported;
                real.CacheSize += this.CacheSize;
                real.CacheObjects += this.CacheObjects;
                real.CacheMisses += this.CacheMisses;
                real.CacheHits += this.CacheHits;
                real.MSecSaved += this.MSecSaved;
            });
        }

        public void Dispose()
        {
            Commit();
        }
    }

    public class CacheInfo : IDisposable, ICacheInfo
    {

        public const string K_Stats = "stats";

        public const string F_StatObjects = "objects.txt";
        public const string F_StatDiskUsage = "usage.txt";
        public const string F_StatHits = "hits.txt";
        public const string F_StatMiss = "misses.txt";
        public const string F_StatUnsupported = "unsupported.txt";

        public const string F_StatSlowHits = "slow_hits.txt";
        public const string F_StatTimeWasted = "time_wasted.txt";
        public const string F_StatTimeSaved = "time_saved.txt";
        public const string F_CacheVersion = "version.txt";

        FileCacheStore cache;
        Mutex statMtx = null;

        public void Commit()
        {

        }

        public bool OmitLocks
        {
            get;
            set;
        }

        public CacheInfo(FileCacheStore statCache)
        {
            cache = statCache;
            statMtx = new Mutex(false, "cclash_stat_" + cache.FolderPath.ToLower().GetHashCode());
        }


        public void LockStatsCall(Action x)
        {
            if ( !OmitLocks ) statMtx.WaitOne();
            x.Invoke();
            if (!OmitLocks) statMtx.ReleaseMutex(); 
        }

        public long ReadStat(string statfile)
        {
            try
            {
                var x = System.IO.File.ReadAllText(cache.MakePath(K_Stats, statfile));
                return Int64.Parse(x);
            }
            catch
            {
                return 0;
            }
        }

        public void WriteStat(string statfile, long value)
        {
            cache.EnsureKey(K_Stats);
            System.IO.File.WriteAllText(cache.MakePath(K_Stats, statfile), value.ToString());
        }



        public long MSecLost
        {
            get
            {
                return ReadStat(F_StatTimeWasted);
            }
            set
            {
                WriteStat(F_StatTimeWasted, value);
            }
        }

        public long MSecSaved
        {
            get
            {
                return ReadStat(F_StatTimeSaved);
            }
            set
            {
                WriteStat(F_StatTimeSaved, value);
            }
        }

        public long SlowHitCount
        {
            get
            {
                return ReadStat(F_StatSlowHits);
            }
            set
            {
                WriteStat(F_StatSlowHits, value);
            }
        }

        public long CacheHits
        {
            get
            {
                return ReadStat(F_StatHits);
            }
            set
            {
                WriteStat(F_StatHits, value);
            }
        }

        public long CacheSize
        {
            get
            {
                return ReadStat(F_StatDiskUsage);
            }
            set
            {
                WriteStat(F_StatDiskUsage, value);
            }
        }

        public long CacheMisses
        {
            get
            {
                return ReadStat(F_StatMiss);
            }
            set
            {
                WriteStat(F_StatMiss, value);
            }
        }

        public long CacheUnsupported
        {
            get
            {
                return ReadStat(F_StatUnsupported);
            }
            set
            {
                WriteStat(F_StatUnsupported, value);
            }
        }

        public long CacheObjects
        {
            get
            {
                return ReadStat(F_StatObjects);
            }
            set
            {
                WriteStat(F_StatObjects, value);
            }
        }

        private void Dispose(bool disposing)
        {
            if ( disposing ) statMtx.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
        }
        
    }
}