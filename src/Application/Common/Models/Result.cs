﻿namespace SDI_Api.Application.Common.Models;

public class Result
{
    internal Result(bool succeeded, IEnumerable<string> errors)
    {
        Succeeded = succeeded;
        Errors = errors.ToArray();
    }

    public bool Succeeded { get; init; }

    public string[] Errors { get; init; }

    public static Result Success()
    {
        return new Result(true, Array.Empty<string>());
    }

    public static Result Failure(IEnumerable<string> errors)
    {
        return new Result(false, errors);
    }
    
    // New generic factory methods
    public static Result<T> Success<T>(T value)
    {
        return new Result<T>(true, Array.Empty<string>(), value);
    }

    public static Result<T> Failure<T>(IEnumerable<string> errors)
    {
        return new Result<T>(false, errors, default!);
    }
}

public class Result<T> : Result
{
    internal Result(bool succeeded, IEnumerable<string> errors, T value)
        : base(succeeded, errors)
    {
        Value = value;
    }

    public T Value { get; init; }
}
