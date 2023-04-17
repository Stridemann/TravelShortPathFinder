namespace TravelShortPathFinder.Algorithm.Data
{
    using System;
    using System.Runtime.CompilerServices;

    [Flags]
    public enum WalkableFlag : byte
    {
        NonWalkable = 1,
        Walkable = 2,
        Processed = 4,
        PossibleSegmentStart = Walkable | Processed | 8,
        PossibleSegment = Walkable | 16,
        PossibleSegmentProcessed = PossibleSegment | Processed,
        FailedCenter = Walkable | Processed | 32
    }

    public static class WalkableFlagExtension
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasMyFlag(this WalkableFlag enm, WalkableFlag flag)
        {
            return (flag & enm) == flag;
        }
    }
}
