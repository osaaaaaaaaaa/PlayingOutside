using MagicOnion;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Interfaces.Services
{
    public interface IMyFirstService : IService<IMyFirstService>
    {
        // ここのスペースに関数形式でどのような関数を作るのか書く

        // 『足し算API』... 二つの整数を引数で受け取り合計を返す
        UnaryResult<int> SumAsync(int x, int f);

        // 『引算API』... 二つの整数を引数で受け取り合計を返す
        UnaryResult<int> SubAsync(int x, int f);

        // 受け取った配列の値の合計を返す
        UnaryResult<int> SumAllAsync(int[] numList);

        // [0]x+y,[1]x-y,[2]x*y,[3]x/yの配列を返す
        UnaryResult<int[]> CalcForOperationAsync(int x,int y);

        /// <summary>
        /// Numberクラスを使ってx+yの結果を返す
        /// </summary>
        /// <param name="numArray">自作クラス『Number』の配列</param>
        /// <returns></returns>
        UnaryResult<float> SumAllNumberAsync(Number numArray);

        /// <summary>
        /// 自作クラス
        /// </summary>
        [MessagePackObject]
        public class Number
        {
            [Key(0)]
            public float x;
            [Key(1)]
            public float y;
        }
    }
}
