using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Utils;

namespace PlayerModelChanger.Services.PlayerBased;

public class PlayerInspectionService : PlayerBasedService {

  private ConfigurationService _ConfigurationService { get; init; }

  private PlayerService _PlayerService { get; init; }

  private ModelService _ModelService { get; init; }

  private CameraStatus? _CameraStatus;

  public PlayerInspectionService(
    int slot,
    ConfigurationService configurationService,
    PlayerService playerService,
    ModelService modelService
    ) : base(slot) {
    _ConfigurationService = configurationService;
    _PlayerService = playerService;
    _ModelService = modelService;
  }

  private const int MAX_ROTATION_TIMES = 300;
  private const float DISTANCE = 70;
  private const float Z_DISTANCE = 50;

  enum CameraMode
  {
    THIRDPERSON, ROTATION
  }

  class CameraStatus
  {
    public CameraMode Mode { get; set; }
    public Vector? Origin { get; set; }
    public CBaseProp? ModelProp { get; set; }
    public required CPhysicsPropMultiplayer CameraProp { get; set; }
    public float Times { get; set; }
  }

  private Vector CalculatePositionInFront(float offSetXY, float offSetZ = 0)
  {
    var pawn = GetPawn();
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

  public void UpdateCamera()
  {
    if (_CameraStatus == null) return;

    if (!_CameraStatus!.CameraProp.IsValid)
    {
      _CameraStatus.CameraProp.Remove();
    }
    if (_CameraStatus!.ModelProp != null && !_CameraStatus.ModelProp.IsValid)
    {
      _CameraStatus.ModelProp.Remove();
    }

    if ((GetPlayer().Buttons & PlayerButtons.Jump) != 0)
    {
      RemoveCamera();
      return;
    }

    var playerPawn = GetPawn();

    if (_CameraStatus!.Mode == CameraMode.ROTATION)
    {
      var origin = _CameraStatus.Origin!;
      float rotationAngle = _CameraStatus.Times / MAX_ROTATION_TIMES * 2 * float.Pi - float.Pi; // - float.PI = from back
      float posX = origin.X + float.Cos(rotationAngle) * DISTANCE;
      float posY = origin.Y + float.Sin(rotationAngle) * DISTANCE;

      var cameraOrigin = new Vector(posX, posY, origin.Z + Z_DISTANCE);

      var cameraAngle = new QAngle(0, 360 * (_CameraStatus.Times / MAX_ROTATION_TIMES), 0);
      _CameraStatus.CameraProp.Teleport(cameraOrigin, cameraAngle, Vector.Zero);
    }
    else if (_CameraStatus.Mode == CameraMode.THIRDPERSON)
    {
      _CameraStatus.CameraProp.Teleport(CalculatePositionInFront(-110, 90), playerPawn.V_angle, Vector.Zero);
    }
    if (_CameraStatus.Times >= MAX_ROTATION_TIMES)
    {
      RemoveCamera();
      return;
    }
    _CameraStatus.Times += 1;


  }

  public void RemoveCamera()
  {
    var player = GetPlayer();
    if (player == null || !player.IsValid || player.PlayerPawn == null || !player.PlayerPawn.IsValid)
    {
      if (_CameraStatus?.CameraProp != null && _CameraStatus.CameraProp.IsValid)
      {
        _CameraStatus.CameraProp.Remove();
      }
      if (_CameraStatus?.Mode == CameraMode.ROTATION)
      {
        if (_CameraStatus?.ModelProp != null && _CameraStatus.ModelProp.IsValid)
        {
          _CameraStatus.ModelProp.Remove();
        }
      }
      return;
    }
    var pawn = GetPawn();
    pawn.CameraServices!.ViewEntity.Raw = uint.MaxValue;
    Utilities.SetStateChanged(pawn, "CBasePlayerPawn", "m_pCameraServices");
    if (_CameraStatus?.CameraProp != null && _CameraStatus.CameraProp.IsValid)
      {
        _CameraStatus.CameraProp.Remove();
      }
      if (_CameraStatus?.Mode == CameraMode.ROTATION)
      {
        if (_CameraStatus?.ModelProp != null && _CameraStatus.ModelProp.IsValid)
        {
          _CameraStatus.ModelProp.Remove();
        }
        pawn.Teleport(_CameraStatus.Origin);
    }
    _CameraStatus = null;
  }

  public void InspectModelForPlayer(string path, Model? model = null)
  {

    RemoveCamera();

    var _cameraProp = Utilities.CreateEntityByName<CPhysicsPropMultiplayer>("prop_physics_multiplayer");

    if (_cameraProp == null || !_cameraProp.IsValid) return;

    _cameraProp.DispatchSpawn();
    _cameraProp.CBodyComponent!.SceneNode!.Owner!.Entity!.Flags = (uint)(_cameraProp.CBodyComponent!.SceneNode!.Owner!.Entity!.Flags & ~(1 << 2));

    _cameraProp.Collision.CollisionGroup = (byte)CollisionGroup.COLLISION_GROUP_NEVER;
    _cameraProp.Collision.SolidFlags = 12;
    _cameraProp.Collision.SolidType = SolidType_t.SOLID_VPHYSICS;

    _cameraProp.Render = Color.FromArgb(0, 255, 255, 255);
    var playerPawn = GetPawn();
    playerPawn.CameraServices!.ViewEntity.Raw = _cameraProp.EntityHandle.Raw;
    Utilities.SetStateChanged(playerPawn, "CBasePlayerPawn", "m_pCameraServices");

    CameraStatus? cameraStatus = null;

    if (_ConfigurationService.ModelConfig.Inspection.Mode == "thirdperson")
    {
      cameraStatus = new CameraStatus
      {
        Mode = CameraMode.THIRDPERSON,
        Origin = playerPawn.AbsOrigin,
        CameraProp = _cameraProp,
        Times = 0
      };

    }
    else if (_ConfigurationService.ModelConfig.Inspection.Mode == "rotation")
    {
      var originLoc = playerPawn.AbsOrigin!;
      var originLocClone = new Vector(originLoc.X, originLoc.Y, originLoc.Z);

      playerPawn.Teleport(new Vector(0, 0, -500));

      CPhysicsPropOverride prop = Utilities.CreateEntityByName<CPhysicsPropOverride>("prop_physics_override")!;
      prop.SetModel(path);
      if (model != null)
      {
        ulong meshgroupmask = Utils.CalculateMeshgroupmask(_ModelService.GetMeshgroupPreference(GetPlayer(), model).ToArray(), model.FixedMeshgroups);
        if (meshgroupmask != 0)
        {
          prop.CBodyComponent!.SceneNode!.GetSkeletonInstance().ModelState.MeshGroupMask = meshgroupmask;
          Utilities.SetStateChanged(prop, "CBaseEntity", "m_CBodyComponent");
        }
      }
      prop.DispatchSpawn();
      _cameraProp.CBodyComponent!.SceneNode!.Owner!.Entity!.Flags = (uint)(_cameraProp.CBodyComponent!.SceneNode!.Owner!.Entity!.Flags & ~(1 << 2));
      // not sure what the fuck is this but it can resolve model have weird pose
      var angle = (180 / float.Pi) * float.Atan2(originLocClone.Y, originLocClone.X) + 180;
      prop.Teleport(originLocClone, new QAngle(0, angle, 0));

      cameraStatus = new CameraStatus
      {
        Mode = CameraMode.ROTATION,
        ModelProp = prop,
        Origin = originLocClone,
        CameraProp = _cameraProp,
        Times = 0
      };

    }
    _CameraStatus = cameraStatus;
  }

  public override void Unload() {
    RemoveCamera();
  }
}