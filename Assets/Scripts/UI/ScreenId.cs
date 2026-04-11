namespace Matchmancer.UI
{
    /// <summary>
    /// Every screen the game can show. <see cref="ScreenNavigator"/> uses
    /// these to manage a history stack. Add new screens at the end — never
    /// reorder — in case future save data references the ordinal.
    /// </summary>
    public enum ScreenId
    {
        None         = 0,

        // ---------- Boot / auth ----------
        Loading      = 1,
        Login        = 2,

        // ---------- Hub ----------
        MainHub      = 10,
        StageSelect  = 11,
        LevelSelect  = 12,

        // ---------- Battle ----------
        Battle       = 20,
        Results      = 21,
        Pause        = 22,

        // ---------- Menus ----------
        Shop         = 30,
        LoreGallery  = 31,
        Achievements = 32,
        Settings     = 33,
        CharacterGear= 34,
    }
}
