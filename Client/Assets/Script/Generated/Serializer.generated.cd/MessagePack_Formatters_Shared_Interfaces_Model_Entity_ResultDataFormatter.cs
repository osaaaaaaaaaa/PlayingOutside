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

namespace MessagePack.Formatters.Shared.Interfaces.Model.Entity
{
    public sealed class ResultDataFormatter : global::MessagePack.Formatters.IMessagePackFormatter<global::Shared.Interfaces.Model.Entity.ResultData>
    {

        public void Serialize(ref global::MessagePack.MessagePackWriter writer, global::Shared.Interfaces.Model.Entity.ResultData value, global::MessagePack.MessagePackSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNil();
                return;
            }

            global::MessagePack.IFormatterResolver formatterResolver = options.Resolver;
            writer.WriteArrayHeader(3);
            global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<global::System.Guid>(formatterResolver).Serialize(ref writer, value.connectionId, options);
            writer.Write(value.rank);
            writer.Write(value.score);
        }

        public global::Shared.Interfaces.Model.Entity.ResultData Deserialize(ref global::MessagePack.MessagePackReader reader, global::MessagePack.MessagePackSerializerOptions options)
        {
            if (reader.TryReadNil())
            {
                return null;
            }

            options.Security.DepthStep(ref reader);
            global::MessagePack.IFormatterResolver formatterResolver = options.Resolver;
            var length = reader.ReadArrayHeader();
            var ____result = new global::Shared.Interfaces.Model.Entity.ResultData();

            for (int i = 0; i < length; i++)
            {
                switch (i)
                {
                    case 0:
                        ____result.connectionId = global::MessagePack.FormatterResolverExtensions.GetFormatterWithVerify<global::System.Guid>(formatterResolver).Deserialize(ref reader, options);
                        break;
                    case 1:
                        ____result.rank = reader.ReadInt32();
                        break;
                    case 2:
                        ____result.score = reader.ReadInt32();
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
