using PlayerModelChanger.Factories;
using PlayerModelChanger.Services.PlayerBased;

namespace PlayerModelChanger.Services;

public class PlayerService {
  private readonly PlayerInspectionServiceFactory _InspectionServiceFactory;

  private List<int> _ManagedPlayers = new();

  private List<PlayerInspectionService> _InspectionServices = new();
  private List<PlayerModelService> _ModelServices = new();

  public PlayerService(
    PlayerInspectionServiceFactory inspectionServiceFactory) {
    _InspectionServiceFactory = inspectionServiceFactory;
  }

  public void AddPlayerServices(int slot) {
    if (_ManagedPlayers.Contains(slot)) {
      return;
    }
    _ManagedPlayers.Add(slot);
    _InspectionServices.Add(_InspectionServiceFactory.Create(slot));
  }

  public void RemovePlayerServices(int slot) {
    if (!_ManagedPlayers.Contains(slot)) {
      return;
    }
    _ManagedPlayers.Remove(slot);
    _InspectionServices.RemoveAll(service => service.Slot == slot);
    _ModelServices.RemoveAll(service => service.Slot == slot);
  }

  public PlayerInspectionService GetInspectionService(int slot) {
    return _InspectionServices.Find(service => service.Slot == slot) ?? throw new Exception("PlayerService::GetInspectionService -> Invalid slot");
  }

  public void Update() {
    foreach (var service in _InspectionServices) {
      service.UpdateCamera();
    }
  }
}