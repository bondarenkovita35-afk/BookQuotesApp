import { HttpClient } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { Quote, QuoteUpsertRequest } from '../models/quote.model';

@Injectable({ providedIn: 'root' })
export class QuoteService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/quotes`;

  readonly quotes = signal<Quote[]>([]);

  loadAll(): Observable<Quote[]> {
    return this.http.get<Quote[]>(this.baseUrl).pipe(tap((quotes) => this.quotes.set(quotes)));
  }

  getById(id: number): Observable<Quote> {
    return this.http.get<Quote>(`${this.baseUrl}/${id}`);
  }

  create(request: QuoteUpsertRequest): Observable<Quote> {
    return this.http.post<Quote>(this.baseUrl, request);
  }

  update(id: number, request: QuoteUpsertRequest): Observable<Quote> {
    return this.http.put<Quote>(`${this.baseUrl}/${id}`, request);
  }

  delete(id: number): Observable<void> {
    return this.http
      .delete<void>(`${this.baseUrl}/${id}`)
      .pipe(tap(() => this.quotes.update((current) => current.filter((quote) => quote.id !== id))));
  }
}
