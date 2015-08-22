using System;

namespace HarshPoint.ShellployGenerator.Builders
{
    public sealed class ParameterBuilderDefaultValue : ParameterBuilder
    {
        internal ParameterBuilderDefaultValue(Object defaultValue)
        {
            DefaultValue = defaultValue;
        }

        public Object DefaultValue { get; }

        protected override void Process(ShellployCommandProperty property)
        {
            property.DefaultValue = DefaultValue;
        }

        public override ParameterBuilder InsertIntoContainer(
            ParameterBuilder existing
        )
        {
            if ((existing != null) &&
                (existing.HasElementsOfType<ParameterBuilderFixed>()))
            {
                throw Logger.Fatal.InvalidOperation(
                    SR.ParameterBuilderDefaultValue_AttemptedToNestFixed
                );
            }

            return base.InsertIntoContainer(existing);
        }

        protected internal override ParameterBuilder Accept(ParameterBuilderVisitor visitor)
        {
            if (visitor == null)
            {
                throw Logger.Fatal.ArgumentNull(nameof(visitor));
            }

            return visitor.VisitDefaultValue(this);
        }

        private static readonly HarshLogger Logger
            = HarshLog.ForContext(typeof(ParameterBuilderDefaultValue));
    }
}
