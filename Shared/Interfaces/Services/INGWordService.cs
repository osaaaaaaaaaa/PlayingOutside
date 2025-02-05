//*********************************************************
// NGワード関係のAPIのインターフェイス
// Author:Rui Enomoto
//*********************************************************
using MagicOnion;
using System;
using System.Collections.Generic;
using System.Text;
using Server.Model.Entity;

namespace Shared.Interfaces.Services
{
    public interface INGWordService : IService<INGWordService>
    {
        /// <summary>
        /// NGワード取得API
        /// </summary>
        /// <returns></returns>
        UnaryResult<string[]> ShowNGWordAsync();
    }
}
