import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiSuccessResponse } from '../models/api-response.model';

export type QueryParams = Record<string, string | number | boolean | undefined | null>;

function toHttpParams(query?: QueryParams): HttpParams {
  let params = new HttpParams();
  if (!query) return params;
  for (const [key, value] of Object.entries(query)) {
    if (value !== undefined && value !== null && value !== '') {
      params = params.set(key, String(value));
    }
  }
  return params;
}

/**
 * Thin HttpClient wrapper that unwraps the backend's `{ success, data }` envelope. Error
 * responses are normalized into ApiError by the error interceptor before they reach callers.
 */
@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiUrl;

  get<T>(path: string, query?: QueryParams): Observable<T> {
    return this.http
      .get<ApiSuccessResponse<T>>(`${this.baseUrl}/${path}`, { params: toHttpParams(query) })
      .pipe(map((res) => res.data));
  }

  post<T>(path: string, body: unknown): Observable<T> {
    return this.http.post<ApiSuccessResponse<T>>(`${this.baseUrl}/${path}`, body).pipe(map((res) => res.data));
  }

  patch<T>(path: string, body: unknown): Observable<T> {
    return this.http.patch<ApiSuccessResponse<T>>(`${this.baseUrl}/${path}`, body).pipe(map((res) => res.data));
  }

  delete<T>(path: string): Observable<T> {
    return this.http.delete<ApiSuccessResponse<T>>(`${this.baseUrl}/${path}`).pipe(map((res) => res.data));
  }
}
