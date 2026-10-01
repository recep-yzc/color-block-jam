namespace ColorBlockJam.Gameplay.Logic
{
    public enum LevelProblemKind
    {
        NoBlocks,
        BlockOutsideBoard,
        BlocksOverlap,
        BlockOnHole,
        HoleTooSmall,
        DoorOutsideBoard,
        DoorsOverlap,
        DoorFacesHole,
        ColorHasNoDoor,
        BlockFitsNoDoor,
        IceNeverMelts
    }
}
