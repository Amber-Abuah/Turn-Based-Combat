namespace RPGCharacterAnims.Lookups
{
    /// <summary>
    /// Enum to use with the "TriggerNumber" parameter of the animator. Convert to (int) to set.
    /// </summary>
    public enum AnimatorTrigger
    {
        NoTrigger = 0,
        IdleTrigger = 1,
        ActionTrigger = 2,
        ClimbLadderTrigger = 3,
        AttackTrigger = 4,
        AttackKickTrigger = 5,
        AttackDualTrigger = 6,
        AttackCastTrigger = 7,
        SpecialAttackTrigger = 8,
        SpecialEndTrigger = 9,
        CastTrigger = 10,
        CastEndTrigger = 11,
        GetHitTrigger = 12,
        RollTrigger = 13,
        TurnTrigger = 14,
        WeaponSheathTrigger = 15,
        WeaponUnsheathTrigger = 16,
        DodgeTrigger = 17,
        JumpTrigger = 18,
        BlockTrigger = 19,
        DeathTrigger = 20,
        ReviveTrigger = 21,
        BlockBreakTrigger = 22,
        SwimTrigger = 23,
        ReloadTrigger = 24,
        InstantSwitchTrigger = 25,
        KnockbackTrigger = 26,
        KnockdownTrigger = 27,
        DiveRollTrigger = 28,
        CrawlTrigger = 29
    }
}