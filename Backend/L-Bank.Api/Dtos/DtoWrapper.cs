using System;

namespace L_Bank.Api.Dtos;

public enum ServiceStatus
{
    Success = 0,
    Failed = 1,
}

public class DtoWrapper<T>
{
    public ServiceStatus Status { get; set; } = ServiceStatus.Success;
    public string Message { get; set; } = "";
    public T? Data { get; set; }
}
