using System;

namespace BookInfo.Details
{
    public static class Status
    {
        private static readonly Random _Random = new();
        public static bool Healthy { get; private set; }
        
        static Status()
        {
            Healthy = _Random.Next(0,100) > 50;
        }
    }
}