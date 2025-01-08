using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Interfaces.Model.Entity
{
    public class ConstantManager
    {
        #region レーティング関係
        public static int DefaultRating { get; private set; } = 1000;
        public static int MaxRating { get; private set; } = 99999;
        #endregion

        // フォロー上限値
        public static int followingCntMax { get; private set; } = 20;
    }
}
