namespace PlayerModelChanger.Services.PlayerBased;

public class PlayerModelService : PlayerBasedService {

  private ConfigurationService _ConfigurationService { get; init; }

  public PlayerModelService(int slot, ConfigurationService configurationService) : base(slot) {
    _ConfigurationService = configurationService;
  }

  public override void Unload() {
  }
}
