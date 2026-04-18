using System.Collections.Generic;
using UnityEngine;

namespace Core.Background
{
    [CreateAssetMenu(menuName = "SuikaGame/Data/Background Picture")]
    public class BackgroundDatabase : ScriptableObject
    {
        public List<BackgroundData> backgroundPictures = new List<BackgroundData>();

        public Sprite GetBackgroundSprite(int backgroundID)
        {
            var backgroundPicture = backgroundPictures.Find(b => b.ID == backgroundID);
            return backgroundPicture != null ? backgroundPicture.Sprite : null;
        }
    }
}
