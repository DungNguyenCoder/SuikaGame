using System;
using System.Collections.Generic;

namespace Core.Skin
{
    [Serializable]
    public class SkinSeries
    {
        public int ID;
        public List<SkinData> skinDatas = new List<SkinData>();
    }
}
