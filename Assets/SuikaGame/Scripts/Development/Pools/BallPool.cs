using System.Collections.Generic;
using SuikaGame.Scripts.Development.Controllers;
using UnityEngine;

namespace SuikaGame.Scripts.Development.Pools
{
    public class BallPool : MonoBehaviour
    {
        [SerializeField] private Ball ballPrefab;
        [SerializeField] private int amount = 50;
        private readonly List<Ball> _tilePool = new List<Ball>();

        private void Awake()
        {
            InitPool();
        }

        private void InitPool()
        {
            for (int i = 0; i < amount; i++)
            {
                CreateNewBall(false);
            }
        }

        private Ball CreateNewBall(bool active)
        {
            Ball newTile = Instantiate(ballPrefab, this.transform);
            newTile.name = "Pooled ball";
            newTile.gameObject.SetActive(active);
            _tilePool.Add(newTile);
            return newTile;
        }

        public Ball GetBall()
        {
            RemoveDestroyedBalls();

            foreach (Ball tile in _tilePool)
            {
                if (!tile.gameObject.activeInHierarchy)
                {
                    tile.gameObject.SetActive(true);
                    return tile;
                }
            }
            return CreateNewBall(true);
        }

        public void ReturnPool(Ball tile)
        {
            if (tile == null) return;

            tile.PrepareForPool();
            tile.gameObject.SetActive(false);
            tile.transform.SetParent(transform);
        }

        public void FillReleasedBalls(List<Ball> output)
        {
            output.Clear();
            RemoveDestroyedBalls();

            foreach (Ball tile in _tilePool)
            {
                if (!tile.gameObject.activeInHierarchy) continue;
                if (!tile.IsReleased) continue;
                output.Add(tile);
            }
        }

        public void ReturnAllReleasedBalls()
        {
            RemoveDestroyedBalls();

            foreach (Ball tile in _tilePool)
            {
                if (!tile.gameObject.activeInHierarchy) continue;
                if (!tile.IsReleased) continue;
                ReturnPool(tile);
            }
        }

        private void RemoveDestroyedBalls()
        {
            for (int i = _tilePool.Count - 1; i >= 0; i--)
            {
                if (_tilePool[i] == null)
                {
                    _tilePool.RemoveAt(i);
                }
            }
        }
    }
}


