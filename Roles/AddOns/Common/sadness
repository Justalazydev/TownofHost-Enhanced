using static TOHE.Options;

namespace TOHE.Roles.AddOns.Common;

public static class Sadness
{
    private const int Id = 21500;
    public static bool IsEnable = false;

    public static OptionItem ImpCanBeSad;
    public static OptionItem CrewCanBeSad;
    public static OptionItem NeutralCanBeSad;
    private static OptionItem SadnessDurationOpt;
    private static OptionItem SadnessReset;
    private static OptionItem LoverDeathSadness;
    private static OptionItem CrewDeathWitnessSadness;

    private static Dictionary<byte, float> SlowedPlayers = new();
    private static Dictionary<byte, Coroutine> SadnessCoroutines = new();

    public static void SetupCustomOptions()
    {
        SetupAdtRoleOptions(Id, CustomRoles.Sadness, canSetNum: true);
        ImpCanBeSad = BooleanOptionItem.Create(Id + 10, "ImpCanBeSad", true, TabGroup.Addons, false).SetParent(CustomRoleSpawnChances[CustomRoles.Sadness]);
        CrewCanBeSad = BooleanOptionItem.Create(Id + 11, "CrewCanBeSad", true, TabGroup.Addons, false).SetParent(CustomRoleSpawnChances[CustomRoles.Sadness]);
        NeutralCanBeSad = BooleanOptionItem.Create(Id + 12, "NeutralCanBeSad", true, TabGroup.Addons, false).SetParent(CustomRoleSpawnChances[CustomRoles.Sadness]);
        SadnessDurationOpt = FloatOptionItem.Create(Id + 13, "SadnessDurationOpt", new(0f, 180f, 1f), 25f, TabGroup.Addons, false).SetParent(CustomRoleSpawnChances[CustomRoles.Sadness])
            .SetValueFormat(OptionFormat.Seconds);
        SadnessReset = BooleanOptionItem.Create(Id + 14, "SadnessReset", true, TabGroup.Addons, false).SetParent(CustomRoleSpawnChances[CustomRoles.Sadness]);
        LoverDeathSadness = BooleanOptionItem.Create(Id + 15, "LoverDeathSadness", true, TabGroup.Addons, false).SetParent(CustomRoleSpawnChances[CustomRoles.Sadness]);
        CrewDeathWitnessSadness = BooleanOptionItem.Create(Id + 16, "CrewDeathWitnessSadness", true, TabGroup.Addons, false).SetParent(CustomRoleSpawnChances[CustomRoles.Sadness]);
    }

    public static void Init()
    {
        SlowedPlayers = new();
        SadnessCoroutines = new();
        IsEnable = false;
    }

    public static void Add()
    {
        IsEnable = true;
    }

    public static void ApplySadness(PlayerControl player)
    {
        if (!SlowedPlayers.ContainsKey(player.PlayerId))
        {
            SlowedPlayers.Add(player.PlayerId, player.MyPhysics.Speed);
            player.MyPhysics.Speed *= 0.1923f; // 520% slower
            Logger.Info($"Player {player.PlayerId} speed reduced to {player.MyPhysics.Speed}", "Sadness");

            if (SadnessCoroutines.ContainsKey(player.PlayerId))
            {
                Destroy(SadnessCoroutines[player.PlayerId]);
            }
            SadnessCoroutines[player.PlayerId] = player.StartCoroutine(RemoveSadnessAfterDuration(player, SadnessDurationOpt.GetFloat()));
        }
    }

    private static IEnumerator RemoveSadnessAfterDuration(PlayerControl player, float duration)
    {
        yield return new WaitForSeconds(duration);
        RemoveSadness(player);
    }

    public static void RemoveSadness(PlayerControl player)
    {
        if (SlowedPlayers.ContainsKey(player.PlayerId))
        {
            player.MyPhysics.Speed = SlowedPlayers[player.PlayerId];
            SlowedPlayers.Remove(player.PlayerId);
            Logger.Info($"Player {player.PlayerId} speed restored to {player.MyPhysics.Speed}", "Sadness");

            if (SadnessCoroutines.ContainsKey(player.PlayerId))
            {
                Destroy(SadnessCoroutines[player.PlayerId]);
                SadnessCoroutines.Remove(player.PlayerId);
            }
        }
    }

    public static void AfterMeetingTasks()
    {
        if (SadnessReset.GetBool())
        {
            foreach (var pid in SlowedPlayers.Keys.ToArray())
            {
                var slowedPlayer = Utils.GetPlayerById(pid);
                if (slowedPlayer == null) continue;
                RemoveSadness(slowedPlayer);
            }
            SlowedPlayers = new();
            SadnessCoroutines = new();
        }
    }

    public static void CheckMurder(PlayerControl killer, PlayerControl victim)
    {
        // Apply sadness to killer
        ApplySadness(killer);

        // Crew witnessing death sadness
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (CrewDeathWitnessSadness.GetBool() && player != killer && player != victim && player.CanSeePlayer(victim))
            {
                ApplySadness(player);
            }
        }
    }

    public static void CheckLoverDeath(PlayerControl lover, PlayerControl loverPartner)
    {
        if (LoverDeathSadness.GetBool())
        {
            ApplySadness(loverPartner);
            loverPartner.StartCoroutine(LoverDeathRoutine(loverPartner));
        }
    }

    private static IEnumerator LoverDeathRoutine(PlayerControl loverPartner)
    {
        yield return new WaitForSeconds(25f); // Customizable duration for sadness effect
        loverPartner.Die();
    }
}
