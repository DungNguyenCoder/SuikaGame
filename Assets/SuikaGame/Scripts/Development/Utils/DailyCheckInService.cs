using System;
using System.Globalization;
using Development.LoadSave.Data;

namespace SuikaGame.Scripts.Development.Utils
{
    public static class DailyCheckInService
    {
        public const int CycleLength = 7;
        private const string DateFormat = "yyyy-MM-dd";

        public static bool CanClaimToday(PlayerSaveData playerData)
        {
            return !IsToday(playerData.LastDailyCheckInDate);
        }

        public static int GetClaimableDay(PlayerSaveData playerData)
        {
            if (playerData.DailyCheckInDay < 1 || playerData.DailyCheckInDay > CycleLength)
            {
                return 1;
            }

            return playerData.DailyCheckInDay;
        }

        public static int GetLastClaimedDay(PlayerSaveData playerData)
        {
            if (string.IsNullOrWhiteSpace(playerData.LastDailyCheckInDate))
            {
                return 0;
            }

            int claimableDay = GetClaimableDay(playerData);
            return claimableDay == 1 ? CycleLength : claimableDay - 1;
        }

        public static void ClaimToday(PlayerSaveData playerData, int rewardAmount)
        {
            int claimedDay = GetClaimableDay(playerData);
            playerData.Coin += rewardAmount;
            playerData.LastDailyCheckInDate = DateTime.Now.ToString(DateFormat, CultureInfo.InvariantCulture);
            playerData.DailyCheckInDay = claimedDay >= CycleLength ? 1 : claimedDay + 1;
        }

        private static bool IsToday(string dateText)
        {
            if (string.IsNullOrWhiteSpace(dateText))
            {
                return false;
            }

            if (!DateTime.TryParseExact(
                    dateText,
                    DateFormat,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime claimedDate))
            {
                return false;
            }

            return claimedDate.Date == DateTime.Now.Date;
        }
    }
}
