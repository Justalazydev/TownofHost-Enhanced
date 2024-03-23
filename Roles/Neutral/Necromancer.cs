﻿using AmongUs.GameOptions;
using System.Collections.Generic;
using static TOHE.Options;
using static TOHE.Translator;

namespace TOHE.Roles.Neutral;

internal class Necromancer : RoleBase
{
    //===========================SETUP================================\\
    private const int Id = 17100;
    private static readonly HashSet<byte> playerIdList = [];
    public static bool HasEnabled => playerIdList.Count > 0;
    public override bool IsEnable => HasEnabled;
    public override CustomRoles ThisRoleBase => CustomRoles.Impostor;
    //==================================================================\\

    private static OptionItem KillCooldown;
    private static OptionItem CanVent;
    private static OptionItem HasImpostorVision;
    private static OptionItem RevengeTime;

    public static PlayerControl Killer = null;
    private static bool IsRevenge = false;
    private static int Timer = 0;
    private static bool Success = false;
    private static float tempKillTimer = 0;

    public static void SetupCustomOption()
    {
        SetupSingleRoleOptions(Id, TabGroup.NeutralRoles, CustomRoles.Necromancer, 1, zeroOne: false);
        KillCooldown = FloatOptionItem.Create(Id + 10, "KillCooldown", new(0f, 180f, 2.5f), 20f, TabGroup.NeutralRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Necromancer])
            .SetValueFormat(OptionFormat.Seconds);
        RevengeTime = IntegerOptionItem.Create(Id + 11, "NecromancerRevengeTime", new(0, 60, 1), 30, TabGroup.NeutralRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Necromancer])
            .SetValueFormat(OptionFormat.Seconds);
        CanVent = BooleanOptionItem.Create(Id + 12, "CanVent", true, TabGroup.NeutralRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Necromancer]);
        HasImpostorVision = BooleanOptionItem.Create(Id + 13, "ImpostorVision", true, TabGroup.NeutralRoles, false).SetParent(CustomRoleSpawnChances[CustomRoles.Necromancer]);
    }
    public override void Init()
    {
        playerIdList.Clear();
        IsRevenge = false;
        Success = false;
        Killer = null;
        tempKillTimer = 0;
    }
    public override void Add(byte playerId)
    {
        playerIdList.Add(playerId);
        Timer = RevengeTime.GetInt();

        if (!AmongUsClient.Instance.AmHost) return;
        if (!Main.ResetCamPlayerList.Contains(playerId))
            Main.ResetCamPlayerList.Add(playerId);
    }
    public override void ApplyGameOptions(IGameOptions opt, byte id) => opt.SetVision(HasImpostorVision.GetBool());
    public override void SetKillCooldown(byte id) => Main.AllPlayerKillCooldown[id] = KillCooldown.GetFloat();
    public override bool CanUseKillButton(PlayerControl pc) => true;
    public override bool CanUseImpostorVentButton(PlayerControl pc) => CanVent.GetBool();
    
    public override bool OnCheckMurderAsTarget(PlayerControl killer, PlayerControl target)
    {
        if (killer.PlayerId == target.PlayerId) return true;
        if (killer == null) return false;
        if (target == null) return false;
        if (IsRevenge) return true;

        _ = new LateTask(() => { target.RpcRandomVentTeleport(); }, 0.01f, "Random Vent Teleport - Necromancer");

        Timer = RevengeTime.GetInt();
        Countdown(Timer, target);
        IsRevenge = true;
        killer.SetKillCooldown();
        killer.Notify(GetString("NecromancerHide"), RevengeTime.GetFloat());
        tempKillTimer = target.killTimer;
        target.SetKillCooldown(time: 1f);
        Killer = killer;

        return false;
    }
    private static void Countdown(int seconds, PlayerControl player)
    {
        var killer = Killer;
        if (Success)
        {
            Timer = RevengeTime.GetInt();
            Success = false;
            Killer = null; 
            return;
        }
        if (seconds <= 0 || GameStates.IsMeeting && player.IsAlive()) 
        { 
            player.RpcMurderPlayerV3(player); 
            player.SetRealKiller(killer);
            Killer = null; 
            return; 
        }
        player.Notify(string.Format(GetString("NecromancerRevenge"), seconds, Killer.GetRealName()), 1.1f);
        Timer = seconds;

        _ = new LateTask(() => { Countdown(seconds - 1, player); }, 1.01f, "Necromancer Countdown");
    }
    public override bool OnCheckMurderAsKiller(PlayerControl killer, PlayerControl target)
    {
        if (killer == null) return false;
        if (target == null) return false;

        if (!IsRevenge) return true;
        else if (target == Killer)
        {
            Success = true;
            killer.Notify(GetString("NecromancerSuccess"));
            killer.SetKillCooldown(KillCooldown.GetFloat() + tempKillTimer);
            IsRevenge = false;
            return true;
        }
        else
        {
            killer.RpcMurderPlayerV3(killer);
            return false;
        }
    }
}
