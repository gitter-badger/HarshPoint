﻿using System;
using System.Collections.Generic;
using SMA = System.Management.Automation;

namespace HarshPoint.ShellployGenerator
{
    internal class CommandParameterInputObject : CommandParameterSynthesized
    {
        internal CommandParameterInputObject()
            : base(typeof(Object), CreateAttributes())
        {
            Name = ShellployCommand.InputObjectPropertyName;
            Position = Int32.MaxValue;
        }

        private static IEnumerable<AttributeData> CreateAttributes()
        {
            yield return new AttributeData(typeof(SMA.ParameterAttribute))
            {
                NamedArguments =
                {
                    ["ValueFromPipeline"]=true
                }
            };
        }
    }
}