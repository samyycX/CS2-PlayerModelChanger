using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Utils;

namespace PlayerModelChanger;

public class ThirdPerson {

    private static List<CameraStatus> cameraStatuses = new List<CameraStatus>();
    const int MAX_ROTATION_TIMES = 300;
    const float DISTANCE = 80;
    const float Z_DISTANCE = 80;

    class CameraStatus {
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
        // rotation is not working
        for (int i = 0; i < cameraStatuses.Count; i++) {
            var cameraStatus = cameraStatuses[i];
            var player = cameraStatus.Player;
            var playerPawn = player.PlayerPawn.Value!;

            var origin = playerPawn.AbsOrigin!;
            float rotationAngle = cameraStatus.Times / MAX_ROTATION_TIMES * 2 * float.Pi - float.Pi; // - float.PI = from back
            // float posX = origin.X + float.Cos(rotationAngle) * DISTANCE;
            // float posY = origin.Y + float.Sin(rotationAngle) * DISTANCE;
            
            // var cameraOrigin = new Vector(posX, posY, origin.Z + Z_DISTANCE);

            // var cameraAngle = new QAngle(Z_DISTANCE / DISTANCE, 360 * (cameraStatus.Times / MAX_ROTATION_TIMES), 0);
            cameraStatus.CameraProp.Teleport(CalculatePositionInFront(player, -110, 90), playerPawn.V_angle, Vector.Zero);
            if (cameraStatus.Times >= MAX_ROTATION_TIMES) {
                playerPawn.CameraServices!.ViewEntity.Raw = uint.MaxValue;
                Utilities.SetStateChanged(playerPawn, "CBasePlayerPawn", "m_pCameraServices");
                cameraStatus.CameraProp.Remove();
                cameraStatuses.RemoveAt(i);
            }
            cameraStatus.Times += 1;

        }

    }

    public static void ThirdPersonPreviewForPlayer(CCSPlayerController player) {

        for (int i = 0; i < cameraStatuses.Count; i++) {
            var oldCameraStatus = cameraStatuses[i];
            var oldPlayer = oldCameraStatus.Player;
            var oldPlayerPawn = oldPlayer.PlayerPawn.Value!;
            oldPlayerPawn.CameraServices!.ViewEntity.Raw = uint.MaxValue;
            Utilities.SetStateChanged(oldPlayerPawn, "CBasePlayerPawn", "m_pCameraServices");
            oldCameraStatus.CameraProp.Remove();
            cameraStatuses.RemoveAt(i);
            break;
        }

        var _cameraProp = Utilities.CreateEntityByName<CPhysicsPropMultiplayer>("prop_physics_multiplayer");

        if (_cameraProp == null || !_cameraProp.IsValid) return;

        _cameraProp.DispatchSpawn();

        _cameraProp.Collision.CollisionGroup = (byte)CollisionGroup.COLLISION_GROUP_NEVER;
        _cameraProp.Collision.SolidFlags = 12;
        _cameraProp.Collision.SolidType = SolidType_t.SOLID_VPHYSICS;

        _cameraProp.Render = Color.FromArgb(0, 255, 255,255);
        var playerPawn = player.PlayerPawn.Value!;
        playerPawn.CameraServices!.ViewEntity.Raw = _cameraProp.EntityHandle.Raw;
        Utilities.SetStateChanged(playerPawn, "CBasePlayerPawn", "m_pCameraServices");

        CameraStatus cameraStatus = new CameraStatus {
            Player = player,
            CameraProp = _cameraProp,
            Times = 0
        };

        cameraStatuses.Add(cameraStatus);
    }
}