using Core.Background;
using UnityEngine;

namespace SuikaGame.Scripts.Development.LoadSave.Data
{
    public class GameplayBackground : MonoBehaviour
    {
        [SerializeField] private BackgroundDatabase backgroundDatabase;
        [SerializeField] private SpriteRenderer backgroundRenderer;

        public void ApplyBackground(int backgroundId)
        {
            Sprite backgroundSprite = backgroundDatabase.GetBackgroundSprite(backgroundId);
            backgroundRenderer.sprite = backgroundSprite;
        }
    }
}
