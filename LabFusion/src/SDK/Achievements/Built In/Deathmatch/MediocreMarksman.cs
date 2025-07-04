﻿namespace LabFusion.SDK.Achievements;

public class MediocreMarksman : KillerAchievement
{
    public override string Title => "Mediocre Marksman";

    public override string Description => "Kill 10 players in Deathmatch or Team Deathmatch.";

    public override int BitReward => 50;

    public override int MaxTasks => 10;
}
