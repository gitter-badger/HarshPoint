﻿using Serilog.Events;
using System;
using System.Linq;

namespace HarshPoint
{
    public sealed class HarshLoggerFatal : HarshLoggerWrapper
    {
        private static readonly HarshLogger SelfLogger = HarshLog.ForContext<HarshLoggerFatal>();

        internal HarshLoggerFatal(HarshLogger inner)
            : base(inner)
        {
        }

        public ArgumentNullException ArgumentNull(String parameterName)
        {
            if (parameterName == null)
            {
                throw SelfLogger.Fatal.ArgumentNull(nameof(parameterName));
            }

            return Write(
                Error.ArgumentNull(parameterName)
            );
        }

        public ArgumentOutOfRangeException ArgumentEmptySequence(String parameterName)
        {
            if (parameterName == null)
            {
                throw SelfLogger.Fatal.ArgumentNull(nameof(parameterName));
            }

            return Write(
                Error.ArgumentOutOfRange_EmptySequence(parameterName)
            );
        }

        public ArgumentOutOfRangeException ArgumentNotAssignableTo(String parameterName, Object value, params Type[] expectedBaseTypes)
        {
            if (parameterName == null)
            {
                throw SelfLogger.Fatal.ArgumentNull(nameof(parameterName));
            }

            if (value == null)
            {
                throw SelfLogger.Fatal.ArgumentNull(nameof(value));
            }

            if (expectedBaseTypes == null)
            {
                throw SelfLogger.Fatal.ArgumentNull(nameof(expectedBaseTypes));
            }

            if (!expectedBaseTypes.Any())
            {
                throw SelfLogger.Fatal.ArgumentEmptySequence(nameof(expectedBaseTypes));
            }

            if (expectedBaseTypes.Length == 1)
            {
                return Write(
                    Error.ArgumentOutOfRangeFormat(
                        parameterName,
                        SR.Error_ObjectNotAssignableToOne,
                        value,
                        expectedBaseTypes
                    )
                );
            }

            return Write(
                Error.ArgumentOutOfRangeFormat(
                    parameterName,
                    SR.Error_ObjectNotAssignableToMany,
                    value,
                    String.Join(", ", expectedBaseTypes.Select(t => t.FullName))
                )
            );
        }

        public ArgumentOutOfRangeException ArgumentOutOfRange(String parameterName, String message)
        {
            if (parameterName == null)
            {
                throw SelfLogger.Fatal.ArgumentNull(nameof(parameterName));
            }

            if (message == null)
            {
                throw SelfLogger.Fatal.ArgumentNull(nameof(message));
            }

            return Write(
                Error.ArgumentOutOfRange(parameterName, message)
            );
        }

        public ArgumentOutOfRangeException ArgumentOutOfRangeFormat(String parameterName, String format, params Object[] args)
        {
            if (parameterName == null)
            {
                throw SelfLogger.Fatal.ArgumentNull(nameof(parameterName));
            }

            if (format == null)
            {
                throw SelfLogger.Fatal.ArgumentNull(nameof(format));
            }

            if (args == null)
            {
                throw SelfLogger.Fatal.ArgumentNull(nameof(args));
            }

            return Write(
                Error.ArgumentOutOfRangeFormat(parameterName, format, args)
            );
        }

        public ArgumentOutOfRangeException ArgumentTypeNotAssignableTo(String parameterName, Type type, Type expectedBaseType)
        {
            if (parameterName == null)
            {
                throw SelfLogger.Fatal.ArgumentNull(nameof(parameterName));
            }

            if (type == null)
            {
                throw SelfLogger.Fatal.ArgumentNull(nameof(type));
            }

            if (expectedBaseType == null)
            {
                throw SelfLogger.Fatal.ArgumentNull(nameof(expectedBaseType));
            }

            return Write(
                Error.ArgumentOutOfRangeFormat(
                    parameterName,
                    SR.Error_TypeNotAssignableFrom,
                    expectedBaseType.FullName,
                    type.FullName
                )
            );
        }

        public InvalidOperationException InvalidOperation(String message)
        {
            if (message == null)
            {
                throw SelfLogger.Fatal.ArgumentNull(nameof(message));
            }

            return Write(
                Error.InvalidOperation(message)
            );
        }

        public InvalidOperationException InvalidOperationFormat(String format, params Object[] args)
        {
            if (format == null)
            {
                throw SelfLogger.Fatal.ArgumentNull(nameof(format));
            }

            if (args == null)
            {
                throw SelfLogger.Fatal.ArgumentNull(nameof(args));
            }

            return Write(
                Error.InvalidOperationFormat(format, args)
            );
        }

        public TException Write<TException>(TException exception)
            where TException : Exception
        {
            return Write(LogEventLevel.Fatal, exception);
        }
    }
}
