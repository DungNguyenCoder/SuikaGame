using System;
using System.Collections.Generic;

namespace SuikaGame.Scripts.Development.LoadSave.Data
{
    [Serializable]
    public class PlayerSaveData
    {
        public string UserName = "Guest";
        public string AvatarId = string.Empty;
        public int Coin;
        public int HighScore;
        public bool HasSeenTutorial;
        public int SelectedSkinSeriesId = 1;
        public int SelectedBackgroundId = 1;
        public string LastDailyCheckInDate = string.Empty;
        public int DailyCheckInDay = 1;
        public List<int> PurchasedSkinSeriesIds = new List<int>();
        public List<int> PurchasedBackgroundIds = new List<int>();
    }
}
