using System;
using System.Diagnostics.CodeAnalysis;

namespace L_Bank.Api.Dtos;

public enum ServiceStatus
{
    Success = 0,
    Failed = 1,
    NotFound = 2,
}

public class DtoWrapper<T>
{
    public static DtoWrapper<T> WrapDto(T data, string? message)
    {
        return new DtoWrapper<T>()
        {
            Status = ServiceStatus.Success,
            Data = data,
            Message = message ?? "",
        };
    }

    public static DtoWrapper<T> WrapDto(ServiceStatus status, string message)
    {
        return new DtoWrapper<T>()
        {
            Status = status,
            Data = default(T),
            Message = message,
        };
    }

    [MemberNotNullWhen(returnValue: true, nameof(Data))]
    public bool IsSuccess => Status == ServiceStatus.Success;

    public ServiceStatus Status { get; set; } = ServiceStatus.Success;
    public string Message { get; set; } = "";
    public T? Data { get; set; }
}
