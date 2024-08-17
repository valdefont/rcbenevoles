namespace web.Utils
{
    using System;

    public class OverlappingPeriod
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        public bool OverlapsWith(OverlappingPeriod otherPeriod)
        {
            // Simple check to see if the two periods overlap
            return Start < otherPeriod.End && otherPeriod.Start < End;
        }
    }
}
