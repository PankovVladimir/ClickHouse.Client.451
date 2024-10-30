using ClickHouse.Client.Formats;
using ClickHouse.Client.Types.Grammar;
using System;
using System.Collections;
using System.Linq;

namespace ClickHouse.Client.Types;

internal class TupleType : ParameterizedType
{
    private Type frameworkType;
    private ClickHouseType[] underlyingTypes;

    public ClickHouseType[] UnderlyingTypes
    {
        get => underlyingTypes;
        set
        {
            underlyingTypes = value;
            frameworkType = DeviseFrameworkType(underlyingTypes);
        }
    }

    private static Type DeviseFrameworkType(ClickHouseType[] underlyingTypes)
    {
        var count = underlyingTypes.Length;

        var typeArgs = new Type[count];
        for (var i = 0; i < count; i++)
        {
            typeArgs[i] = underlyingTypes[i].FrameworkType;
        }
        var genericType = Type.GetType("System.Tuple`" + typeArgs.Length);
        return genericType.MakeGenericType(typeArgs);
    }



    public override Type FrameworkType => frameworkType;

    public override string Name => "Tuple";

    public override ParameterizedType Parse(SyntaxTreeNode node, Func<SyntaxTreeNode, ClickHouseType> parseClickHouseTypeFunc, TypeSettings settings)
    {
        return new TupleType
        {
            UnderlyingTypes = node.ChildNodes.Select(parseClickHouseTypeFunc).ToArray(),
        };
    }

    public override string ToString() => $"{Name}({string.Join(",", UnderlyingTypes.Select(t => t.ToString()))})";

    public override object Read(ExtendedBinaryReader reader)
    {
        var count = UnderlyingTypes.Length;
        var contents = new object[count];
        for (var i = 0; i < count; i++)
        {
            var value = UnderlyingTypes[i].Read(reader);
            contents[i] = ClearDBNull(value);
        }
        return contents;
    }

    public override void Write(ExtendedBinaryWriter writer, object value)
    {
        if (value is IList list)
        {
            if (list.Count != UnderlyingTypes.Length)
                throw new ArgumentException("Wrong number of elements in Tuple", nameof(value));
            for (var i = 0; i < list.Count; i++)
            {
                UnderlyingTypes[i].Write(writer, list[i]);
            }
            return;
        }
    }
}
