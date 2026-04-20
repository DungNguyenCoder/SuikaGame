using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Development.LoadSave.Data
{
    [Serializable]
    public class BallSaveData
    {
        [FormerlySerializedAs("ID")] public int BallId;
        public float PositionX;
        public float PositionY;
        public float VelocityX;
        public float VelocityY;
        public float AngularVelocity;

        public BallSaveData()
        {
        }

        public BallSaveData(int id, Vector2 position)
            : this(id, position, Vector2.zero, 0f)
        {
        }

        public BallSaveData(int id, Vector2 position, Vector2 velocity, float angularVelocity)
        {
            BallId = id;
            PositionX = position.x;
            PositionY = position.y;
            VelocityX = velocity.x;
            VelocityY = velocity.y;
            AngularVelocity = angularVelocity;
        }

        public Vector2 GetPosition()
        {
            return new Vector2(PositionX, PositionY);
        }

        public Vector2 GetVelocity()
        {
            return new Vector2(VelocityX, VelocityY);
        }
    }
}
