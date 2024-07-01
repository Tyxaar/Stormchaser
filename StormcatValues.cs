using System;
using System.Security.Permissions;
using UnityEngine;

/*
 * This file contains fixes to some common problems when modding Rain World.
 * Unless you know what you're doing, you shouldn't modify anything here.
 */

// Allows access to private members
#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618

namespace Stormcat
{
    public class PlayerValues
    {
        public PlayerValues(Player self)
        {
            playerRef = new WeakReference<Player>(self);
        }

        public WeakReference<Player> playerRef;
        public int[] armFlapMeshIndices = new int[2];
        public int glideCooldown = 80;
        public int zoomDirection;
        public int zoomSpeed;
        public bool playerGliding;
        public bool canDoubleJump;
        public bool canGlide;

        public Creature.Grasp[] grasps
        {
            get
            {
                playerRef.TryGetTarget(out var player);
                return player.grasps;
            }
        }

        public Color tailColour;
        public Color bodyColour;
        public Color eyeColour;

        public bool holdingGlide => playerRef.TryGetTarget(out var player) && player.input[0].jmp && player.canJump == 0 && player.canWallJump == 0;
        public bool holdingBigItem => playerRef.TryGetTarget(out var player) && player != null && player.grasps[0]?.grabbed is TubeWorm || player.Grabability(player.grasps[0]?.grabbed) is Player.ObjectGrabability.TwoHands || player.Grabability(player.grasps[1]?.grabbed) is Player.ObjectGrabability.TwoHands;
        public bool rechargeGlide
        {
            get
            {
                playerRef.TryGetTarget(out var player);
                string[] bodyModes = { "Stand", "Crawl", "CorridorClimb", "Swimming", "ClimbingOnBeam" };
                for (int i = 0; i < bodyModes.Length; i++)
                {
                    Player.BodyModeIndex bodyRef = new Player.BodyModeIndex(bodyModes[i]);
                    if (player.bodyMode == bodyRef)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
        public bool touchingTerrain => playerRef.TryGetTarget(out var player) && player != null && (player.bodyChunks[0].contactPoint != default || player.bodyChunks[1].contactPoint != default || player.bodyMode != Player.BodyModeIndex.Default || player.animation == Player.AnimationIndex.Flip || rechargeGlide);
        public bool triggerGlide => playerRef.TryGetTarget(out var player) && glideCooldown == 0 && !touchingTerrain && canDoubleJump && player.input[0].jmp && !player.input[1].jmp;

    }
}

internal static class StormcatValues
{
    private static bool _initialized;

    // Ensure resources are only loaded once and that failing to load them will not break other mods
    public static On.RainWorld.hook_OnModsInit WrapInit(Action<RainWorld> loadResources)
    {
        return (orig, self) =>
        {
            orig(self);

            try
            {
                if (!_initialized)
                {
                    _initialized = true;
                    loadResources(self);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        };
    }
}

public static class Scenes
{
    public static Menu.MenuScene.SceneID Dream_Stormchaser_Alone = new(nameof(Dream_Stormchaser_Alone), false);
    public static Menu.MenuScene.SceneID Dream_Stormchaser_Beacon = new(nameof(Dream_Stormchaser_Beacon), false);
    public static Menu.MenuScene.SceneID Dream_Stormchaser_Thief = new(nameof(Dream_Stormchaser_Thief), false);
    public static Menu.MenuScene.SceneID Dream_Stormchaser_Wayfarers = new(nameof(Dream_Stormchaser_Wayfarers), false);
}

public static class Dreams
{
    public static DreamsState.DreamID Dream_Stormchaser_Alone = new(nameof(Dream_Stormchaser_Alone), true);
    public static DreamsState.DreamID Dream_Stormchaser_Beacon = new(nameof(Dream_Stormchaser_Beacon), true);
    public static DreamsState.DreamID Dream_Stormchaser_Thief = new(nameof(Dream_Stormchaser_Thief), true);
    public static DreamsState.DreamID Dream_Stormchaser_Wayfarers = new(nameof(Dream_Stormchaser_Wayfarers), true);
}