using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;

namespace PlayerModelChanger.Services.PlayerBased;

public abstract class PlayerBasedService {

  public int Slot { get; init; }

  public PlayerBasedService(int slot) {
    Slot = slot;
  }

  public CCSPlayerController GetPlayer() {
    return Utilities.GetPlayerFromSlot(Slot) ?? throw new Exception("PlayerBasedService::GetPlayer -> Invalid player");
  }

  public CCSPlayerPawn GetPawn() {
    return GetPlayer().PlayerPawn.Value!;
  }

  public abstract void Unload();
}