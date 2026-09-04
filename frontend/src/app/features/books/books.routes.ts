import { Routes } from '@angular/router';

export const BOOKS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./book-list/book-list').then((m) => m.BookList),
  },
  {
    path: 'new',
    loadComponent: () => import('./book-form/book-form').then((m) => m.BookForm),
  },
  {
    path: ':id/edit',
    loadComponent: () => import('./book-form/book-form').then((m) => m.BookForm),
  },
];
