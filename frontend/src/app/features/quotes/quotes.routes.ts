import { Routes } from '@angular/router';

export const QUOTES_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./quote-list/quote-list').then((m) => m.QuoteList),
  },
  {
    path: 'new',
    loadComponent: () => import('./quote-form/quote-form').then((m) => m.QuoteForm),
  },
  {
    path: ':id/edit',
    loadComponent: () => import('./quote-form/quote-form').then((m) => m.QuoteForm),
  },
];
