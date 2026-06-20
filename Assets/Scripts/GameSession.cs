public enum GameState
{
    None, Continue, NewGame
}
public static class GameSession
{
    public static GameState CurrentGameState = GameState.None;
    public static bool SessionStarted = false;
    public static bool ShouldLoadFromFile()
    {
        if (CurrentGameState == GameState.NewGame)
            return false;

        return SaveSystem.LoadGame();
    }
}