using MagicOnion;
using MagicOnion.Server;
using Server.Model.Context;
using Server.Model.Entity;
using Shared.Interfaces.Model.Entity;
using Shared.Interfaces.Services;
using System.Xml.Linq;

namespace Server.Services
{
    public class NGWordService : ServiceBase<INGWordService>, INGWordService
    {
        /// <summary>
        /// NGワード取得API
        /// </summary>
        /// <returns></returns>
        public async UnaryResult<string[]> ShowNGWordAsync()
        {
            using var context = new GameDbContext();
            var ngWords = context.NG_Words.Select(n => n.Word).ToArray();

            // バリデーションチェック
            if (ngWords == null)
            {
                throw new ReturnStatusException(Grpc.Core.StatusCode.InvalidArgument, "NGワードを取得できません");
            }

            return ngWords;
        }
    }
}
