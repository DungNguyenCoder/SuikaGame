using System.Collections.Generic;
using UnityEngine;

public class BallPool : MonoBehaviour
{
    [SerializeField] private Ball ballPrefab;
    [SerializeField] private int amount = 30;
    private List<Ball> _tilePool = new List<Ball>();

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
        if (!_tilePool.Contains(tile)) return;
        tile.gameObject.SetActive(false);
        tile.transform.SetParent(transform);
    }
}


