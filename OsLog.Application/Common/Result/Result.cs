using System;
using System.Collections.Generic;
using System.Text;

namespace OsLog.Application.Common.Result;

public class Result
{
    public bool IsSuccess { get; }
    public IReadOnlyList<AppError> Errors { get; }

    protected Result(bool isSuccess, List<AppError>? errors = null)
    {
        IsSuccess = isSuccess;
        Errors = errors ?? new List<AppError>();
    }

    public static Result Ok() => new(true);
    public static Result Fail(params AppError[] errors) => new(false, errors.ToList());

    public int GetHttpStatusOrDefault(int successStatus = 200)
        => IsSuccess ? successStatus : Errors.Select(e => e.ResolveHttpStatus()).DefaultIfEmpty(400).Max();
}

public sealed class Result<T> : Result
{
    public T? Value { get; }

    private Result(bool isSuccess, T? value, List<AppError>? errors = null)
        : base(isSuccess, errors)
    {
        Value = value;
    }

    public static Result<T> Ok(T value) => new(true, value);
    public static Result<T> Fail(params AppError[] errors) => new(false, default, errors.ToList());
}

