import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

/**
 * Om ett 401-svar kommer när användaren redan var inloggad betyder det att
 * sessionen har gått ut eller blivit ogiltig (inte ett misslyckat inloggningsförsök,
 * då finns det ju ingen session än) — då loggas användaren ut och skickas till /login.
 */
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401 && authService.isLoggedIn()) {
        authService.logout();
        router.navigate(['/login'], { queryParams: { sessionExpired: true } });
      }

      return throwError(() => error);
    }),
  );
};
