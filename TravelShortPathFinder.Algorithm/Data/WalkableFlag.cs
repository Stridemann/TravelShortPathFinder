namespace TravelShortPathFinder.Algorithm.Data
{
    using System;
    using System.Runtime.CompilerServices;

    [Flags]
    public enum WalkableFlag : byte
    {
        NonWalkable = 1,
        Walkable = 2,
        Passed = 4,
        PossibleSegmentStart = Walkable | Passed | 8,
        PossibleSegment = Walkable | 16,
        PossibleSegmentPassed = PossibleSegment | Passed,
        FailedCenter = Walkable | Passed | 32
    }

    public static class WalkableFlagExtension
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Contain(this WalkableFlag enm, WalkableFlag flag)
        {
            return (flag & enm) == flag;
        }
    }
}
