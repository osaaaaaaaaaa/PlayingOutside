using MagicOnion.Client;
using MessagePack;
using MessagePack.Resolvers;
using MessagePack.Unity;

/// <summary>
/// MagicOnion逕ｨ繧､繝ｳ繧ｿ繝輔ぉ繝ｼ繧ｹ縺ｮ繧ｳ繝ｼ繝臥函謌�
/// </summary>
[MagicOnionClientGeneration(typeof(Shared.Interfaces.Services.IMyFirstService))]
partial class MagicOnionInitializer
{
    /// <summary>
    /// Resolver縺ｮ逋ｻ骭ｲ蜃ｦ逅�
    /// </summary>
    [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void RegisterResolvers()
    {
        StaticCompositeResolver.Instance.Register(
            MagicOnionInitializer.Resolver,
            GeneratedResolver.Instance,
            BuiltinResolver.Instance,
            UnityResolver.Instance,
            PrimitiveObjectResolver.Instance
        );

        MessagePackSerializer.DefaultOptions = MessagePackSerializer.DefaultOptions
            .WithResolver(StaticCompositeResolver.Instance);
    }
}