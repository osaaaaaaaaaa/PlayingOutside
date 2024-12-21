// <auto-generated>
// THIS (.cs) FILE IS GENERATED BY MPC(MessagePack-CSharp). DO NOT CHANGE IT.
// </auto-generated>

#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 168
#pragma warning disable CS1591 // document public APIs

#pragma warning disable SA1312 // Variable names should begin with lower-case letter
#pragma warning disable SA1649 // File name should match first type name

namespace MessagePack.Resolvers
{
    public class GeneratedResolver : global::MessagePack.IFormatterResolver
    {
        public static readonly global::MessagePack.IFormatterResolver Instance = new GeneratedResolver();

        private GeneratedResolver()
        {
        }

        public global::MessagePack.Formatters.IMessagePackFormatter<T> GetFormatter<T>()
        {
            return FormatterCache<T>.Formatter;
        }

        private static class FormatterCache<T>
        {
            internal static readonly global::MessagePack.Formatters.IMessagePackFormatter<T> Formatter;

            static FormatterCache()
            {
                var f = GeneratedResolverGetFormatterHelper.GetFormatter(typeof(T));
                if (f != null)
                {
                    Formatter = (global::MessagePack.Formatters.IMessagePackFormatter<T>)f;
                }
            }
        }
    }

    internal static class GeneratedResolverGetFormatterHelper
    {
        private static readonly global::System.Collections.Generic.Dictionary<global::System.Type, int> lookup;

        static GeneratedResolverGetFormatterHelper()
        {
            lookup = new global::System.Collections.Generic.Dictionary<global::System.Type, int>(8)
            {
                { typeof(global::Shared.Interfaces.Model.Entity.GameScene.SCENE_ID), 0 },
                { typeof(global::Server.Model.Entity.User), 1 },
                { typeof(global::Shared.Interfaces.Model.Entity.GameScene), 2 },
                { typeof(global::Shared.Interfaces.Model.Entity.JoinedUser), 3 },
                { typeof(global::Shared.Interfaces.Model.Entity.PlayerState), 4 },
                { typeof(global::Shared.Interfaces.Model.Entity.ResultData), 5 },
                { typeof(global::Shared.Interfaces.Model.Entity.UserState), 6 },
                { typeof(global::Shared.Interfaces.Services.IMyFirstService.Number), 7 },
            };
        }

        internal static object GetFormatter(global::System.Type t)
        {
            int key;
            if (!lookup.TryGetValue(t, out key))
            {
                return null;
            }

            switch (key)
            {
                case 0: return new MessagePack.Formatters.Shared.Interfaces.Model.Entity.GameScene_SCENE_IDFormatter();
                case 1: return new MessagePack.Formatters.Server.Model.Entity.UserFormatter();
                case 2: return new MessagePack.Formatters.Shared.Interfaces.Model.Entity.GameSceneFormatter();
                case 3: return new MessagePack.Formatters.Shared.Interfaces.Model.Entity.JoinedUserFormatter();
                case 4: return new MessagePack.Formatters.Shared.Interfaces.Model.Entity.PlayerStateFormatter();
                case 5: return new MessagePack.Formatters.Shared.Interfaces.Model.Entity.ResultDataFormatter();
                case 6: return new MessagePack.Formatters.Shared.Interfaces.Model.Entity.UserStateFormatter();
                case 7: return new MessagePack.Formatters.Shared.Interfaces.Services.IMyFirstService_NumberFormatter();
                default: return null;
            }
        }
    }
}

#pragma warning restore 168
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612

#pragma warning restore SA1312 // Variable names should begin with lower-case letter
#pragma warning restore SA1649 // File name should match first type name
