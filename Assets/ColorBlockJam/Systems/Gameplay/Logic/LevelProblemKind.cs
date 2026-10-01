namespace ColorBlockJam.Gameplay.Logic
{
    public enum LevelProblemKind
    {
        NoBlocks,
        BlockOutsideBoard,
        BlocksOverlap,
        BlockOnHole,
        DoorOutsideBoard,
        DoorsOverlap,
        DoorFacesHole,
        ColorHasNoDoor,
        BlockFitsNoDoor,
        IceNeverMelts
    }
}
