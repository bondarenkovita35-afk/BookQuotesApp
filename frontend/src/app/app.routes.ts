import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { guestGuard } from './core/guards/guest.guard';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'books' },
  {
    path: 'login',
    canActivate: [guestGuard],
    loadComponent: () => import('./features/auth/login/login').then((m) => m.Login),
  },
  {
    path: 'register',
    canActivate: [guestGuard],
    loadComponent: () => import('./features/auth/register/register').then((m) => m.Register),
  },
  {
    path: 'books',
    canActivate: [authGuard],
    loadChildren: () => import('./features/books/books.routes').then((m) => m.BOOKS_ROUTES),
  },
  {
    path: 'quotes',
    canActivate: [authGuard],
    loadChildren: () => import('./features/quotes/quotes.routes').then((m) => m.QUOTES_ROUTES),
  },
  { path: '**', redirectTo: 'books' },
];
