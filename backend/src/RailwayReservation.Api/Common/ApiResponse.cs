namespace RailwayReservation.Api.Common;

/// <summary>Consistent response envelope used by every endpoint in the API — see README "API Conventions".</summary>
public sealed record ApiSuccessResponse<T>(bool Success, T Data);

public sealed record ApiErrorBody(string Code, string Message, object? Details = null);

public sealed record ApiErrorResponse(bool Success, ApiErrorBody Error);
