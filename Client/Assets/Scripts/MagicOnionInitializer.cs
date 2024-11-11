using MagicOnion.Client;
using MessagePack;
using MessagePack.Resolvers;
using MessagePack.Unity;

/// <summary>
/// MagicOnion用インタフェースのコード生成
/// </summary>
[MagicOnionClientGeneration(typeof(Shared.Interfaces.Services.IMyFirstService))]
partial class MagicOnionInitializer
{
    /// <summary>
    /// Resolverの登録処理
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