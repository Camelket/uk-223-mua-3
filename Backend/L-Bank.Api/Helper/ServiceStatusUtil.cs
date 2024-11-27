using System.Net;
using L_Bank.Api.Dtos;

public static class ServiceStatusUtil
{
    public static int Map(ServiceStatus status)
    {
        var statusCode = status switch
        {
            ServiceStatus.Success => HttpStatusCode.OK,
            ServiceStatus.NotFound => HttpStatusCode.NotFound,
            ServiceStatus.Failed => HttpStatusCode.InternalServerError,
            _ => HttpStatusCode.InternalServerError,
        };
        return (int)statusCode;
    }
}
