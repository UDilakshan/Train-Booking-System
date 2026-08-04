namespace RailwayReservation.Application.Common.Exceptions;

/// <summary>
/// Base for business-rule violations raised from use-cases. The Api layer's exception-handling
/// middleware maps each subclass to an HTTP status via <see cref="StatusCode"/>, and serializes
/// Code/Message/Details into the standard error envelope — see README "API Conventions".
/// </summary>
public abstract class AppException(int statusCode, string code, string message, object? details = null) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
    public string Code { get; } = code;
    public object? Details { get; } = details;
}

public sealed class NotFoundAppException(string code, string message) : AppException(404, code, message);

public class ConflictAppException(string code, string message, object? details = null) : AppException(409, code, message, details);

public sealed class ValidationAppException(string message, object? details = null) : AppException(400, "VALIDATION_ERROR", message, details);

public sealed class UnauthorizedAppException(string code, string message) : AppException(401, code, message);

public sealed class SegmentOverlapException(string seatNumber)
    : ConflictAppException("SEGMENT_OVERLAP", $"Seat {seatNumber} is already booked for an overlapping part of this journey.");

public sealed class InvalidSegmentException(string message) : ConflictAppException("INVALID_SEGMENT", message);
