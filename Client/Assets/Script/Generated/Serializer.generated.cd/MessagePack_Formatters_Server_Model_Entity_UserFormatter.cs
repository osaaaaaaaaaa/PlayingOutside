// <auto-generated>
// THIS (.cs) FILE IS GENERATED BY MPC(MessagePack-CSharp). DO NOT CHANGE IT.
// </auto-generated>

#pragma warning disable 618
#pragma warning disable 612
#pragma warning disable 414
#pragma warning disable 168
#pragma warning disable CS1591 // document public APIs

#pragma warning disable SA1129 // Do not use default value type constructor
#pragma warning disable SA1309 // Field names should not begin with underscore
#pragma warning disable SA1312 // Variable names should begin with lower-case letter
#pragma warning disable SA1403 // File may only contain a single namespace
#pragma warning disable SA1649 // File name should match first type name

namespace MessagePack.Formatters.Server.Model.Entity
{
    public sealed class UserFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::Server.Model.Entity.User>
    {

        public void Serialize(ref global::MessagePack.MessagePackWriter writer, global::Server.Model.Entity.User value, global::MessagePack.MessagePackSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNil();
                return;
            }

            global::MessagePack.IFormatterResolver formatterResolver = options.Resolver;
            writer.WriteArrayHeader(5);
            writer.Write(value.Id);
            global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<string>(formatterResolver).Serialize(ref writer, value.Name, options);
            global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<string>(formatterResolver).Serialize(ref writer, value.Token, options);
            global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<global::System.DateTime>(formatterResolver).Serialize(ref writer, value.Created_at, options);
            global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<global::System.DateTime>(formatterResolver).Serialize(ref writer, value.Updated_at, options);
        }

        public global::Server.Model.Entity.User Deserialize(ref global::MessagePack.MessagePackReader reader, global::MessagePack.MessagePackSerializerOptions options)
        {
            if (reader.TryReadNil())
            {
                return null;
            }

            options.Security.DepthStep(ref reader);
            global::MessagePack.IFormatterResolver formatterResolver = options.Resolver;
            var length = reader.ReadArrayHeader();
            var ____result = new global::Server.Model.Entity.User();

            for (int i = 0; i < length; i++)
            {
                switch (i)
                {
                    case 0:
                        ____result.Id = reader.ReadInt32();
                        break;
                    case 1:
                        ____result.Name = global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<string>(formatterResolver).Deserialize(ref reader, options);
                        break;
                    case 2:
                        ____result.Token = global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<string>(formatterResolver).Deserialize(ref reader, options);
                        break;
                    case 3:
                        ____result.Created_at = global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<global::System.DateTime>(formatterResolver).Deserialize(ref reader, options);
                        break;
                    case 4:
                        ____result.Updated_at = global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<global::System.DateTime>(formatterResolver).Deserialize(ref reader, options);
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            reader.Depth--;
            return ____result;
        }
    }

}

#pragma warning restore 168
#pragma warning restore 414
#pragma warning restore 618
#pragma warning restore 612

#pragma warning restore SA1129 // Do not use default value type constructor
#pragma warning restore SA1309 // Field names should not begin with underscore
#pragma warning restore SA1312 // Variable names should begin with lower-case letter
#pragma warning restore SA1403 // File may only contain a single namespace
#pragma warning restore SA1649 // File name should match first type name
