using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace RailwayReservation.Api.Common.Filters;

/// <summary>Wraps every successful controller return value in the standard `{ success, data }` envelope.</summary>
public sealed class ApiResponseWrappingFilter : IAsyncResultFilter
{
    public Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.Result is ObjectResult { Value: not null } objectResult && objectResult.Value is not ApiErrorResponse)
        {
            var wrapperType = typeof(ApiSuccessResponse<>).MakeGenericType(objectResult.Value.GetType());
            objectResult.Value = Activator.CreateInstance(wrapperType, true, objectResult.Value);
            // ObjectResult.DeclaredType still holds the pre-wrap type (e.g. LoginResult) from the
            // action's return type. Left unset, the JSON formatter resolves serialization metadata
            // for that stale type against the new ApiSuccessResponse<T> value and throws
            // InvalidCastException. Only bites single-object (non-collection) action results —
            // collection-returning actions happen to route through a different formatter path —
            // which is why this surfaced on /auth/login and not /stations.
            objectResult.DeclaredType = wrapperType;
        }

        return next();
    }
}
