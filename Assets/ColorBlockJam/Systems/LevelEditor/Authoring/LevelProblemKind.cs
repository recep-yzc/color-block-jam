namespace ColorBlockJam.LevelEditor.Authoring
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
        IceNeverMelts,
        EmptyBlock
    }
}
