import { HttpClient } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { Book, BookUpsertRequest } from '../models/book.model';

@Injectable({ providedIn: 'root' })
export class BookService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/books`;

  readonly books = signal<Book[]>([]);

  loadAll(): Observable<Book[]> {
    return this.http.get<Book[]>(this.baseUrl).pipe(tap((books) => this.books.set(books)));
  }

  getById(id: number): Observable<Book> {
    return this.http.get<Book>(`${this.baseUrl}/${id}`);
  }

  create(request: BookUpsertRequest): Observable<Book> {
    return this.http.post<Book>(this.baseUrl, request);
  }

  update(id: number, request: BookUpsertRequest): Observable<Book> {
    return this.http.put<Book>(`${this.baseUrl}/${id}`, request);
  }

  delete(id: number): Observable<void> {
    return this.http
      .delete<void>(`${this.baseUrl}/${id}`)
      .pipe(tap(() => this.books.update((current) => current.filter((book) => book.id !== id))));
  }
}
