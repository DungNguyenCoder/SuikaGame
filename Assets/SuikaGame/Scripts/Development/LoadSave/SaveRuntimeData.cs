using Development.LoadSave.Data;

namespace Development.LoadSave
{
    public static class SaveRuntimeData
    {
        public static PlayerSaveData Player { get; private set; }
        public static ProgressSaveData Progress { get; private set; }

        public static void SetPlayer(PlayerSaveData data)
        {
            Player = data;
        }

        public static void SetProgress(ProgressSaveData data)
        {
            Progress = data;
        }
    }
}
