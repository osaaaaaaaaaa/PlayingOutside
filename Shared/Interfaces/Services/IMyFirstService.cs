using MagicOnion;
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
    }
}
