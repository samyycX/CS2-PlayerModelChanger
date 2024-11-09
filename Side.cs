namespace PlayerModelChanger;

public enum Side
{
  All,
  T,
  CT

}

public static class SideExtensions
{
  public static string ToName(this Side side)
  {
    return side switch
    {
      Side.All => "all",
      Side.T => "t",
      Side.CT => "ct",
      _ => throw new ArgumentOutOfRangeException(nameof(side), side, "Invalid Side value")
    };
  }

  public static Side? ToSide(this string sideString)
  {
    return sideString.ToLower() switch
    {
      "all" => Side.All,
      "t" => Side.T,
      "ct" => Side.CT,
      _ => null
    };
  }
}