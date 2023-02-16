namespace TravelShortPathFinder.Algorithm.Data
{
    using System;
    using System.Runtime.CompilerServices;

    [Flags]
    public enum WalkableFlag : byte
    {
        Nonwalkable = 1,
        Walkable = 2,
        Passed = 4,
        PossibleSectorMarkedPassed = Walkable | Passed | 8,
        PossibleSector = Walkable | 16,
        SectorCenter = PossibleSector | Passed,
        FailedCenter = Walkable | Passed | 32,
        LockedObstacle = 64,
        UnlockedObstacle = 128
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
