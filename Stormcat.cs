using System;
using BepInEx;
using UnityEngine;
using SlugBase.Features;
using static SlugBase.Features.FeatureTypes;
using System.Runtime.CompilerServices;
using SlugBase.DataTypes;
using RWCustom;
using JetBrains.Annotations;
using IL;
using On;
using Random = UnityEngine.Random;
using DevInterface;
using System.Security.Cryptography.X509Certificates;
using Mono.Cecil;

namespace Stormcat
{
    [BepInPlugin(MOD_ID, "Stormchaser", "0.1.0")]
    partial class Stormcat : BaseUnityPlugin
    {
        private const string MOD_ID = "vela.stormchaser";

        public static SlugcatStats.Name Stormchaser = new SlugcatStats.Name("Stormchaser");

        //Grab the colour features
        public static readonly PlayerFeature<ColorSlot[]> CustomColors;

        //Set up the CWT
        public static ConditionalWeakTable<Player, PlayerValues> PlayerData = new ConditionalWeakTable<Player, PlayerValues>();
        public static PlayerValues Data(Player player) => PlayerData.GetValue(player, p => new PlayerValues(p));


        // Add hooks
        public void OnEnable()
        {
            On.RainWorld.OnModsInit += StormcatValues.WrapInit(LoadResources);

            //Gameplay hooks
            On.Player.MovementUpdate += Player_MovementUpdate;
            On.Player.Jump += Player_Jump;
            //Graphics hooks
            On.PlayerGraphics.AddToContainer += PlayerGraphics_AddToContainer;
            On.PlayerGraphics.InitiateSprites += PlayerGraphics_InitiateSprites;
            On.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites;
            On.PlayerGraphics.ApplyPalette += PlayerGraphics_ApplyPalette;
            On.PlayerGraphics.Update += PlayerGraphics_Update;
            //Misc hooks
            On.RainCycle.ctor += RainCycle_ctor;
            On.RainWorldGame.Update += RainWorldGame_Update; //Room shaking camera transitions
                                                             //Quest hooks
            On.SSOracleBehavior.PebblesConversation.AddEvents += PebblesConversation_AddEvents;
            //echo hooks
            On.GhostWorldPresence.SpawnGhost += GhostWorldPresence_SpawnGhost;
            StormyPassageHooks.Apply();
        }

        private bool GhostWorldPresence_SpawnGhost(On.GhostWorldPresence.orig_SpawnGhost orig, GhostWorldPresence.GhostID ghostID, int karma, int karmaCap, int ghostPreviouslyEncountered, bool playingAsRed)
        {
            //If the ghost is the UW ghost, change the ID to the CC echo
            //Since this method is only responsible for deciding whether an echo should spawn or not, changing the ID to the CC echo
            //will cause the echo to share the same spawning rules as the CC echo (i.e. requiring priming) without affecting any other aspects of it
            if (Custom.rainWorld.progression.currentSaveState.saveStateNumber == Stormchaser && ghostID == GhostWorldPresence.GhostID.UW)
            {
                ghostID = GhostWorldPresence.GhostID.CC;
            }

            return orig(ghostID, karma, karmaCap, ghostPreviouslyEncountered, playingAsRed);
        }

        public void RainWorldGame_Update(On.RainWorldGame.orig_Update orig, RainWorldGame self)
        {
            orig(self);
            if (self.processActive && !self.GamePaused && self.cameras[0].room != null)
            {
                string myRoom = self.cameras[0].room.roomSettings.name.ToString();
                if (myRoom == "NC_A02" || myRoom == "NC_C01" || myRoom == "NC_B01" || myRoom == "NC_A03" || myRoom == "NC_A04" || myRoom == "NC_A05" || myRoom == "NC_B02" || myRoom == "NC_A06")
                {
                    self.cameras[0].screenShake += 0.1f;
                }
                //self.cameras[0].screenShake += 0.2f;
            }
        }

        private void RainCycle_ctor(On.RainCycle.orig_ctor orig, RainCycle self, World world, float minutes)
        {
            if (world.game.session is StoryGameSession session && session.saveStateNumber == Stormchaser)
            {
                const float min1 = 2.5f;
                const float max1 = 4f;

                const float min2 = 8.5f;
                const float max2 = 10f;

                minutes = Random.value < 0.5f ? Mathf.Lerp(min1, max1, Random.value) : Mathf.Lerp(min2, max2, Random.value);
            }
            orig(self, world, minutes);
        }

        private void PlayerGraphics_AddToContainer(On.PlayerGraphics.orig_AddToContainer orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            orig(self, sLeaser, rCam, newContatiner);

            var data = Data(self.player);

            if (self.player.slugcatStats.name != Stormchaser)
                return;

            //var fg = rCam.ReturnFContainer("Foreground");
            var mg = rCam.ReturnFContainer("Midground");

            for (int i = 0; i < 2; i++)
            {
                //fg.RemoveChild(sLeaser.sprites[data.armFlapMeshIndices[i]]);
                sLeaser.sprites[data.armFlapMeshIndices[i]].RemoveFromContainer();
                mg.AddChild(sLeaser.sprites[data.armFlapMeshIndices[i]]);
            }
        }

        private void PlayerGraphics_InitiateSprites(On.PlayerGraphics.orig_InitiateSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            var data = Data(self.player);

            data.armFlapMeshIndices = new int[2];

            orig(self, sLeaser, rCam);

            if (self.player.slugcatStats.name != Stormchaser)
                return;

            int oldSLeaserLength = sLeaser.sprites.Length;

            Array.Resize(ref sLeaser.sprites, oldSLeaserLength + 2);
            sLeaser.sprites[oldSLeaserLength] = TriangleMesh.MakeLongMesh(2, false, false);
            sLeaser.sprites[oldSLeaserLength + 1] = TriangleMesh.MakeLongMesh(2, false, false);

            data.armFlapMeshIndices[0] = oldSLeaserLength;
            data.armFlapMeshIndices[1] = oldSLeaserLength + 1;

            self.AddToContainer(sLeaser, rCam, null);
        }

        private void PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);

            if (self.player.slugcatStats.name != Stormchaser)
                return;

            var data = Data(self.player);

            TriangleMesh[] flapMeshes = { (TriangleMesh)sLeaser.sprites[data.armFlapMeshIndices[0]], (TriangleMesh)sLeaser.sprites[data.armFlapMeshIndices[1]] };

            Vector2 upperFlapAnchorPos = sLeaser.sprites[0].GetPosition();

            for (int i = 0; i < 2; i++)
            {
                Vector2 handFlapAnchorPos = sLeaser.sprites[5 + i].GetPosition();

                Vector2 bodyOrientation = Custom.DirVec(self.player.bodyChunks[1].pos, self.player.bodyChunks[0].pos);
                float upness = Math.Max(Vector2.Dot(bodyOrientation, new Vector2(0, 1)), 0f);

                Vector2 lowerFlapAnchorPos = Vector2.Lerp(sLeaser.sprites[4].GetPosition(), sLeaser.sprites[1].GetPosition(), Mathf.Lerp(0.1f, 0.4f, upness));
                lowerFlapAnchorPos += Custom.PerpendicularVector(bodyOrientation) * Mathf.Lerp(0, 4, upness) * (i == 0 ? 1 : -1) - (bodyOrientation * Mathf.Lerp(10, 0, upness));

                float armStraightness = (float.Parse(sLeaser.sprites[5 + i].element.name.Substring(9)) - 1) / 12; // this is so funny

                Vector2 innerElbowFlapAnchorPos = Vector2.Lerp(upperFlapAnchorPos, handFlapAnchorPos, 0.75f) + Custom.RotateAroundOrigo(Custom.DirVec(upperFlapAnchorPos, handFlapAnchorPos), -90 * (i == 0 ? 1 : -1)) * 5f * (1f - armStraightness);

                Vector2 flapFloppyBezierControl = innerElbowFlapAnchorPos - bodyOrientation * 12f * (0.9f + 0.3f * armStraightness);

                flapMeshes[i].vertices[0] = upperFlapAnchorPos;
                flapMeshes[i].vertices[1] = lowerFlapAnchorPos;
                flapMeshes[i].vertices[2] = Vector2.Lerp(upperFlapAnchorPos, innerElbowFlapAnchorPos, 0.5f);
                flapMeshes[i].vertices[3] = Custom.Bezier(lowerFlapAnchorPos, flapFloppyBezierControl, handFlapAnchorPos, flapFloppyBezierControl, 0.33f);
                flapMeshes[i].vertices[4] = innerElbowFlapAnchorPos;
                flapMeshes[i].vertices[5] = Custom.Bezier(lowerFlapAnchorPos, flapFloppyBezierControl, handFlapAnchorPos, flapFloppyBezierControl, 0.66f);
                flapMeshes[i].vertices[6] = handFlapAnchorPos;
                flapMeshes[i].vertices[7] = handFlapAnchorPos;

                Debug.Log(armStraightness);

                flapMeshes[i].isVisible = sLeaser.sprites[5 + i].isVisible;
            }
        }

        private void PlayerGraphics_Update(On.PlayerGraphics.orig_Update orig, PlayerGraphics self)
        {
            orig(self);
            var data = Data(self.player);

            if (self.player.slugcatStats.name != Stormchaser || self.lightSource == null)
            { return; }
            if (self.lightSource != null)
            {
                data.eyeColour = new PlayerColor("Eyes").GetColor(self) ?? Color.cyan;
                self.lightSource.color = data.eyeColour;
                self.lightSource.alpha = 0.25f;
            }
        }

        //Make Storm's jumps big, including pole jumps
        void Player_Jump(On.Player.orig_Jump orig, Player self)
        {
            if (self.slugcatStats.name == Stormchaser)
            {
                self.jumpBoost *= 1.2f;
                if (self.animation == Player.AnimationIndex.ClimbOnBeam)
                {
                    self.jumpBoost *= 0f;
                    float num = Mathf.Lerp(1f, 1.15f, self.Adrenaline);
                    if (self.input[0].x != 0)
                    {
                        self.animation = Player.AnimationIndex.None;
                        self.bodyChunks[0].vel.y = 7f * num;
                        self.bodyChunks[1].vel.y = 6f * num;
                        self.bodyChunks[0].vel.x = 7f * self.flipDirection * num;
                        self.bodyChunks[1].vel.x = 5f * self.flipDirection * num;
                    }
                }
            }
            orig(self);
            if (self.slugcatStats.name == Stormchaser && (self.animation != Player.AnimationIndex.ClimbOnBeam))
            {
                self.jumpBoost *= 1.2f;
            }

        }

        public void Player_MovementUpdate(On.Player.orig_MovementUpdate orig, Player self, bool eu)
        {
            if (self.SlugCatClass != Stormchaser)
            {
                orig(self, eu);
                return;
            }
            var data = Data(self);

            const float normalGravity = 0.9f;
            const float boostPower = 10;



            //Double Jump boost in 0g, adapted from the Artificer code.
            if (self.input[0].jmp && self.bodyMode == Player.BodyModeIndex.ZeroG && self.animation == Player.AnimationIndex.ZeroGSwim && data.glideCooldown == 0)
            {
                //Make fwoosh sound
                self.room.PlaySound(SoundID.Slugcat_Normal_Jump, self.mainBodyChunk.pos, 1f, 1f);

                //Set cooldown
                data.glideCooldown = 40;

                //Leap!
                float jumpX = self.input[0].x;
                float jumpY = self.input[0].y;
                while (jumpX == 0f && jumpY == 0f)
                {
                    jumpX = (UnityEngine.Random.value <= 0.33) ? 0 : ((UnityEngine.Random.value <= 0.5) ? 1 : -1);
                    jumpY = (UnityEngine.Random.value <= 0.33) ? 0 : ((UnityEngine.Random.value <= 0.5) ? 1 : -1);
                }
                self.bodyChunks[0].vel.x = 9f * jumpX;
                self.bodyChunks[0].vel.y = 9f * jumpY;
                self.bodyChunks[1].vel.x = 8f * jumpX;
                self.bodyChunks[1].vel.y = 8f * jumpY;
            }

            //Initiate double jump 
            if (data.triggerGlide && self.bodyMode != Player.BodyModeIndex.ZeroG && !data.holdingBigItem)
            {
                //make fwoosh sound
                data.canDoubleJump = false;
                self.room.PlaySound(SoundID.Slugcat_Normal_Jump, self.mainBodyChunk.pos, 1f, 1f);

                //Boost!
                foreach (BodyChunk chunk in self.bodyChunks)
                {
                    chunk.vel.y = boostPower;
                }
                //Set up variables for gliding.
                data.canGlide = true;
                data.playerGliding = true;
                data.zoomSpeed = 0;
            }

            //If statement that runs when gliding.
            if (data.holdingGlide && data.canGlide && data.playerGliding && self.mainBodyChunk.vel.y < 0f && !data.touchingTerrain)
            {
                //Apply glide physics changes
                self.slugOnBack.interactionLocked = true;

                if (self.slugOnBack != null && self.slugOnBack.slugcat != null && self.slugOnBack.HasASlug)
                {
                    foreach (BodyChunk chunk in self.slugOnBack.slugcat.bodyChunks)
                    {
                        chunk.vel.y = Custom.LerpAndTick(chunk.vel.y, -0.5f, 0.2f, 0.5f);
                    }
                }
                self.animation = Player.AnimationIndex.RocketJump;
                foreach (BodyChunk chunk in self.bodyChunks)
                {
                    chunk.vel.y = Custom.LerpAndTick(chunk.vel.y, -0.5f, 0.2f, 0.5f);
                    //Stop velocity on turning
                    if (self.input[0].y + self.input[0].x != 0)
                    {
                        if (self.flipDirection != data.zoomDirection)
                        {
                            self.animation = Player.AnimationIndex.CrawlTurn;
                            data.zoomSpeed = 0;
                            chunk.vel.x = 0;
                            data.zoomDirection = self.flipDirection;
                        }
                        else
                        //Increase zoomies but cap it at 50
                        {
                            if (data.zoomSpeed > 50)
                            {
                                data.zoomSpeed = 50;
                            }
                            data.zoomSpeed++;
                        }
                        //Activate zoomies speed increase
                        if (data.canGlide && Mathf.Abs(chunk.vel.x) < 20)
                        {
                            chunk.vel.x = Mathf.Lerp(chunk.vel.x, chunk.vel.x + (data.zoomSpeed / 20 * self.flipDirection), 0.2f);
                        }
                        else
                        {
                            chunk.vel.x = 20 * self.flipDirection;
                        }
                    }
                    else
                    {
                        //Slow down if not moving forwards
                        chunk.vel.x = Mathf.Lerp(chunk.vel.x, 0, 0.2f);
                        self.animation = Player.AnimationIndex.None;
                    }

                }
            }

            //Recharge the glide when on a wall, floor, or pole.
            if (data.rechargeGlide && self.room.gravity != 0)
            {
                self.gravity = normalGravity;
                self.customPlayerGravity = normalGravity;
                data.canDoubleJump = true;
                data.playerGliding = false;
                data.canGlide = true;
                data.zoomSpeed = 0;
            }

            //Make glide cooldown tick down.
            if (data.glideCooldown > 0)
            {
                data.glideCooldown--;
            }
            //Cooldown for how soon you can jump after getting off the ground. Mostly used to fix 0g problems.
            if (data.glideCooldown < 5 && data.rechargeGlide && self.bodyMode == Player.BodyModeIndex.ZeroG && self.animation == Player.AnimationIndex.ZeroGSwim)
            {
                data.glideCooldown = 5;
            }

            //Logs
            //Debug.Log("Glide Cooldown" + data.glideCooldown);
            orig(self, eu);
        }

        //Colour changes
        void PlayerGraphics_ApplyPalette(On.PlayerGraphics.orig_ApplyPalette orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            orig(self, sLeaser, rCam, palette);
            var data = Data(self.player);

            if (self.player.slugcatStats.name != Stormchaser)
            { return; }

            //Set colours
            data.bodyColour = new PlayerColor("Body").GetColor(self) ?? Color.blue;
            data.eyeColour = new PlayerColor("Eyes").GetColor(self) ?? Color.cyan;
            data.tailColour = new PlayerColor("Tail").GetColor(self) ?? Color.cyan;


            //Change Mark of Communication colour   
            sLeaser.sprites[11].color = data.eyeColour;
            sLeaser.sprites[10].color = data.eyeColour;
            //And the tail colour
            sLeaser.sprites[2].color = data.tailColour;
            //Also the shortcut


            //Make the tail gradient
            if (sLeaser.sprites[2] is TriangleMesh tailMesh)
            {
                sLeaser.sprites[2].MoveBehindOtherNode(sLeaser.sprites[1]);
                if (tailMesh.verticeColors == null || tailMesh.verticeColors.Length != tailMesh.vertices.Length)
                {
                    tailMesh.verticeColors = new Color[tailMesh.vertices.Length];
                }
                tailMesh.customColor = true;

                var color2 = data.bodyColour; //Base color
                var color3 = data.tailColour; //Tip color

                for (int j = tailMesh.verticeColors.Length - 1; j >= 0; j--)
                {
                    float num = (j / 2f) / (tailMesh.verticeColors.Length - 3 / 2f);
                    if (j > 13)
                        tailMesh.verticeColors[j] = data.tailColour;
                    else if (j < 2)
                        tailMesh.verticeColors[j] = data.bodyColour;
                    else
                        tailMesh.verticeColors[j] = Color.Lerp(color2, color3, num);

                }
                tailMesh.Refresh();
            }
        }

        // Load any resources, such as sprites or sounds
        private void LoadResources(RainWorld rainWorld)
        {
            //Futile.atlasManager.LoadImage("atlases/StormAtlas"); //LoadImage NOTABLY DIFFERENT THAN LoadAtlas
            Futile.atlasManager.LoadAtlas("atlases/StormAtlas");
        }
    }
}