import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideAnimations } from '@angular/platform-browser/animations';
import { provideRouter } from '@angular/router';

import { authInterceptor } from './core/interceptors/auth.interceptor';
import { errorInterceptor } from './core/interceptors/error.interceptor';
import { routes } from './app.routes';

// Zone.js-based change detection (not zoneless — Angular Material 22's mat-form-field content
// projection has a timing bug under provideZonelessChangeDetection() at the time of writing:
// mat-select/mat-label projected content isn't detected, throwing
// "mat-form-field must contain a MatFormFieldControl". Tracked upstream; revisit once fixed.
export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideAnimations(),
    provideHttpClient(withInterceptors([authInterceptor, errorInterceptor])),
  ],
};
