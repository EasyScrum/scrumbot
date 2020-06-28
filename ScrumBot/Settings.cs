namespace ScrumBot
{
    public class Settings
    {
        //TODO move it to settings!
#if DEBUG
        public static bool UseTeams = false;
#else
        public static bool UseTeams = true;
#endif
    }
}
