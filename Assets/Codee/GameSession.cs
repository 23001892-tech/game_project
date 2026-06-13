public enum GameState
{
    None, Continue, NewGame
}
public static class GameSession
{
    public static GameState CurrentGameState = GameState.None;
    public static bool SessionStarted = false;
}