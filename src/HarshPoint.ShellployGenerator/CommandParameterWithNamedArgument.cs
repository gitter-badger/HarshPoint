﻿using System;
using System.Linq;

namespace HarshPoint.ShellployGenerator
{
    internal sealed class CommandParameterWithNamedArgument : CommandParameter
    {
        public CommandParameterWithNamedArgument(
            Type attributeType, 
            String name, 
            Object value
        )
        {
            AttributeType = attributeType;
            Name = name;
            Value = value;
        }

        public Type AttributeType { get; }
        public String Name { get; }
        public Object Value { get; }

        internal override void Process(ShellployCommandProperty property)
        {
            var attributes = property.Attributes.Where(
                a => a.AttributeType == AttributeType
            );

            foreach (var attr in attributes)
            {
                attr.NamedArguments[Name] = Value;
            }
        }
    }
}
