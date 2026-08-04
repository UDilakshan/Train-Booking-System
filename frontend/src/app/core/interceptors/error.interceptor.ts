import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';
import { ApiError, ApiErrorResponse } from '../models/api-response.model';

/** Converts the backend's `{ success: false, error: { code, message } }` body into a typed ApiError. */
export const errorInterceptor: HttpInterceptorFn = (req, next) =>
  next(req).pipe(
    catchError((err: unknown) => {
      if (err instanceof HttpErrorResponse) {
        const body = err.error as ApiErrorResponse | undefined;
        if (body && body.success === false && body.error) {
          return throwError(() => new ApiError(body.error.code, body.error.message, body.error.details));
        }
        return throwError(() => new ApiError('NETWORK_ERROR', err.message || 'Network error occurred.'));
      }
      return throwError(() => err);
    }),
  );
