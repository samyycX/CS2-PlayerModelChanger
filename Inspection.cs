using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Utils;

namespace PlayerModelChanger;

public class Inspection {

    private static List<CameraStatus> cameraStatuses = new List<CameraStatus>();
    const int MAX_ROTATION_TIMES = 300;
    const float DISTANCE = 70;
    const float Z_DISTANCE = 50;

    enum CameraMode {
        THIRDPERSON, ROTATION
    }

    class CameraStatus {
        public CameraMode Mode { get; set; }
        public Vector? Origin { get; set; }
        public CBaseProp? ModelProp { get; set; }
        public CCSPlayerController Player { get; set; }
        public CPhysicsPropMultiplayer CameraProp { get; set; }
        public float Times { get; set; }
    }

    public static Vector CalculatePositionInFront(CCSPlayerController player, float offSetXY, float offSetZ = 0)
    {
        var pawn = player.PlayerPawn.Value;
        // Extract yaw angle from player's rotation QAngle
        float yawAngle = pawn!.EyeAngles!.Y;

        // Convert yaw angle from degrees to radians
        float yawAngleRadians = (float)(yawAngle * Math.PI / 180.0);

        // Calculate offsets in x and y directions
        float offsetX = offSetXY * (float)Math.Cos(yawAngleRadians);
        float offsetY = offSetXY * (float)Math.Sin(yawAngleRadians);

        // Calculate position in front of the player
        var positionInFront = new Vector
        {
            X = pawn!.AbsOrigin!.X + offsetX,
            Y = pawn!.AbsOrigin!.Y + offsetY,
            Z = pawn!.AbsOrigin!.Z + offSetZ
        };

        return positionInFront;
    }

    public static void UpdateCamera() {
        for (int i = 0; i < cameraStatuses.Count; i++) {
            var cameraStatus = cameraStatuses[i];
            var player = cameraStatus.Player;

            if ((player.Buttons & PlayerButtons.Jump) != 0) {
                RemoveCamera(player);
                continue;
            }

            var playerPawn = player.PlayerPawn.Value!;
            
            if (cameraStatus.Mode == CameraMode.ROTATION) {
                var origin = cameraStatus.Origin;
                float rotationAngle = cameraStatus.Times / MAX_ROTATION_TIMES * 2 * float.Pi - float.Pi; // - float.PI = from back
                float posX = origin.X + float.Cos(rotationAngle) * DISTANCE;
                float posY = origin.Y + float.Sin(rotationAngle) * DISTANCE;
                
                var cameraOrigin = new Vector(posX, posY, origin.Z + Z_DISTANCE);

                var cameraAngle = new QAngle(0, 360 * (cameraStatus.Times / MAX_ROTATION_TIMES), 0);
                cameraStatus.CameraProp.Teleport(cameraOrigin, cameraAngle, Vector.Zero);
            } else if (cameraStatus.Mode == CameraMode.THIRDPERSON) {
                cameraStatus.CameraProp.Teleport(CalculatePositionInFront(player, -110, 90), playerPawn.V_angle, Vector.Zero);
            }
            if (cameraStatus.Times >= MAX_ROTATION_TIMES) {
                RemoveCamera(player);
            }
            cameraStatus.Times += 1;

        }

    }

    public static void RemoveCamera(CCSPlayerController player) {
         for (int i = 0; i < cameraStatuses.Count; i++) {
            var cameraStatus = cameraStatuses[i];
            var oldPlayer = cameraStatus.Player;
            if (oldPlayer.SteamID != player.SteamID) {
                continue;
            }
            var oldPlayerPawn = oldPlayer.PlayerPawn.Value!;
            oldPlayerPawn.CameraServices!.ViewEntity.Raw = uint.MaxValue;
            Utilities.SetStateChanged(oldPlayerPawn, "CBasePlayerPawn", "m_pCameraServices");
            cameraStatus.CameraProp.Remove();
            cameraStatuses.RemoveAt(i);
            if (cameraStatus.Mode == CameraMode.ROTATION) {
                cameraStatus.ModelProp.Remove();
                oldPlayerPawn.Teleport(cameraStatus.Origin);
            }
            break;
        }
    }

    public static void InspectModelForPlayer(CCSPlayerController player, string model) {

        RemoveCamera(player);

        var _cameraProp = Utilities.CreateEntityByName<CPhysicsPropMultiplayer>("prop_physics_multiplayer");

        if (_cameraProp == null || !_cameraProp.IsValid) return;

        _cameraProp.DispatchSpawn();

        _cameraProp.Collision.CollisionGroup = (byte)CollisionGroup.COLLISION_GROUP_NEVER;
        _cameraProp.Collision.SolidFlags = 12;
        _cameraProp.Collision.SolidType = SolidType_t.SOLID_VPHYSICS;

        _cameraProp.Render = Color.FromArgb(0,255,255,255);
        var playerPawn = player.PlayerPawn.Value!;
        playerPawn.CameraServices!.ViewEntity.Raw = _cameraProp.EntityHandle.Raw;
        Utilities.SetStateChanged(playerPawn, "CBasePlayerPawn", "m_pCameraServices");

        CameraStatus? cameraStatus = null;

        if (PlayerModelChanger.INSTANCE!.Config.Inspection.Mode == "thirdperson") {
            cameraStatus = new CameraStatus {
                Mode = CameraMode.THIRDPERSON,
                Origin = playerPawn.AbsOrigin,
                Player = player,
                CameraProp = _cameraProp,
                Times = 0
            };

        } else if (PlayerModelChanger.INSTANCE!.Config.Inspection.Mode == "rotation") {
            var originLoc = playerPawn.AbsOrigin!;
            var originLocClone = new Vector(originLoc.X, originLoc.Y, originLoc.Z);
            
            playerPawn.Teleport(new Vector(0, 0, -500));

            CPhysicsPropOverride? prop = Utilities.CreateEntityByName<CPhysicsPropOverride>("prop_physics_override");
            prop.SetModel(model);
            prop.DispatchSpawn();
            // not sure what the fuck is this but it can resolve model have weird pose
            var angle = (180 / float.Pi) * float.Atan2(originLocClone.Y, originLocClone.X) + 180; 
            prop.Teleport(originLocClone, new QAngle(0, angle, 0));

            cameraStatus = new CameraStatus {
                Mode = CameraMode.ROTATION,
                ModelProp = prop,
                Origin = originLocClone,
                Player = player,
                CameraProp = _cameraProp,
                Times = 0
            };
        }

        cameraStatuses.Add(cameraStatus!);
    }
}