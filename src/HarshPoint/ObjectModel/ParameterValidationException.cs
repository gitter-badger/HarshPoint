﻿using System;

namespace HarshPoint.ObjectModel
{
    public class ParameterValidationException : Exception
    {
        public ParameterValidationException() { }

        public ParameterValidationException(String message)
            : base(message)
        {
        }
        public ParameterValidationException(String message, Exception innerException)
            : base(message, innerException)
        {
        }

        public ParameterValidationException(String parameterName, String message)
            : base(message)
        {
            if (String.IsNullOrWhiteSpace(parameterName))
            {
                throw Logger.Fatal.ArgumentNullOrWhiteSpace(nameof(parameterName));
            }

            ParameterName = parameterName;
        }

        public ParameterValidationException(String parameterName, String message, Exception inner)
            : base(message, inner)
        {
            if (String.IsNullOrWhiteSpace(parameterName))
            {
                throw Logger.Fatal.ArgumentNullOrWhiteSpace(nameof(parameterName));
            }

            ParameterName = parameterName;
        }

        public String ParameterName { get; }

        private static readonly HarshLogger Logger = HarshLog.ForContext<ParameterValidationException>();
    }
}
