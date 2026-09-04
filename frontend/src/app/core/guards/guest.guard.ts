import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

/**
 * Skickar bort en redan inloggad användare från inloggnings- och
 * registreringssidorna istället för att visa dem på nytt.
 */
export const guestGuard: CanActivateFn = () => {
  const authService = inject(AuthService);
  const router = inject(Router);

  return authService.isLoggedIn() ? router.createUrlTree(['/books']) : true;
};
