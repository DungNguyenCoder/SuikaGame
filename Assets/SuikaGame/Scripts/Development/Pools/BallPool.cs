using System;
using System.Collections.Generic;
using Development.Controllers;
using UnityEngine;

namespace Development.Pools
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
            tile.PrepareForPool();
            tile.gameObject.SetActive(false);
            tile.transform.SetParent(transform);
        }

        public void FillReleasedBalls(List<Ball> output)
        {
            output.Clear();
            foreach (Ball tile in _tilePool)
            {
                if (!tile.gameObject.activeInHierarchy) continue;
                if (!tile.IsReleased) continue;
                output.Add(tile);
            }
        }

        public void ReturnAllReleasedBalls()
        {
            foreach (Ball tile in _tilePool)
            {
                if (!tile.gameObject.activeInHierarchy) continue;
                if (!tile.IsReleased) continue;
                ReturnPool(tile);
            }
        }
    }
}


