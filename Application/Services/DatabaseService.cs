using PlayerModelChanger.Services.Storage;

namespace PlayerModelChanger.Services;

public class DatabaseService {

  private ConfigurationService _ConfigurationService;

  private PluginMetadataProviderService _PluginMetadataProviderService;

  private IStorage _Storage;

  public DatabaseService(
    ConfigurationService configurationService,
    PluginMetadataProviderService pluginMetadataProviderService) {
    _ConfigurationService = configurationService;
    _PluginMetadataProviderService = pluginMetadataProviderService;

    Initialize();
  }

  public void Initialize() {
    var config = _ConfigurationService.ModelConfig;


    if (config.StorageType == "mysql")
    {
      if (config.MySQLIP == "")
      {
        throw new Exception("[PlayerModelChanger] You must fill in the MySQL_IP");
      }
      if (config.MySQLPort == "")
      {
        throw new Exception("[PlayerModelChanger] You must fill in the MYSQL_Port");
      }
      if (config.MySQLUser == "")
      {
        throw new Exception("[PlayerModelChanger] You must fill in the MYSQL_User");
      }
      if (config.MySQLPassword == "")
      {
        throw new Exception("[PlayerModelChanger] You must fill in the MYSQL_Password");
      }
      if (config.MySQLDatabase == "")
      {
        throw new Exception("[PlayerModelChanger] You must fill in the MySQL_Database");
      }
    }
    
    switch (config.StorageType)
    {
      case "sqlite":
        _Storage = new SqliteStorage(_PluginMetadataProviderService.GetPluginMetadata().ModuleDirectory);
        break;
      case "mysql":
        _Storage = new MySQLStorage(config.MySQLIP, config.MySQLPort, config.MySQLUser, config.MySQLPassword, config.MySQLDatabase, config.MySQLTable);
        break;
    }
  }

  public IStorage GetStorage() {
    return _Storage;
  }
}