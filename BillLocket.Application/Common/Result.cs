using System;
using System.Collections.Generic;
using System.Text;

namespace BillLocket.Application.Common
{
    public sealed class Result<T>
    {
        private Result(
            bool succeeded,
            T? value,
            AppError? error)
        {
            Succeeded = succeeded;
            Value = value;
            Error = error;
        }

        public bool Succeeded { get; }
        public T? Value { get; }
        public AppError? Error { get; }

        public static Result<T> Success(T value)
        {
            ArgumentNullException.ThrowIfNull(value);

            return new Result<T>(true, value, null);
        }

        public static Result<T> Failure(AppError error)
        {
            ArgumentNullException.ThrowIfNull(error);

            return new Result<T>(false, default, error);
        }
    }
}
