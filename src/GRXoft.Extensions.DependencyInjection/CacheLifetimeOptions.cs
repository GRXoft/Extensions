using System;
using System.Collections.Generic;

namespace GRXoft.Extensions.DependencyInjection
{
    public class CacheLifetimeOptions
    {
        public IList<TimeSpan> RetryIntervals { get; set; } = new List<TimeSpan>();

        public TimeSpan UpdateInterval { get; set; }
    }
}
