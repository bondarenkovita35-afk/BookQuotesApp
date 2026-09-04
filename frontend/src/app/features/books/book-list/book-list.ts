import { Component, OnInit, inject, signal, viewChild } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ConfirmDialog } from '../../../shared/components/confirm-dialog/confirm-dialog';
import { BookService } from '../services/book.service';

@Component({
  selector: 'app-book-list',
  imports: [RouterLink, ConfirmDialog],
  templateUrl: './book-list.html',
})
export class BookList implements OnInit {
  private readonly bookService = inject(BookService);
  private readonly confirmDialog = viewChild.required(ConfirmDialog);

  readonly books = this.bookService.books;
  readonly isLoading = signal(true);
  readonly errorMessage = signal<string | null>(null);

  private bookIdPendingDelete: number | null = null;

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.bookService.loadAll().subscribe({
      next: () => this.isLoading.set(false),
      error: () => {
        this.isLoading.set(false);
        this.errorMessage.set('Det gick inte att hämta böckerna. Försök igen senare.');
      },
    });
  }

  askDelete(id: number, title: string): void {
    this.bookIdPendingDelete = id;
    this.confirmDialog().open('Ta bort bok', `Vill du ta bort "${title}"? Det går inte att ångra.`);
  }

  confirmDelete(): void {
    if (this.bookIdPendingDelete === null) {
      return;
    }

    const id = this.bookIdPendingDelete;
    this.bookIdPendingDelete = null;

    this.bookService.delete(id).subscribe({
      error: () => this.errorMessage.set('Det gick inte att ta bort boken. Försök igen.'),
    });
  }
}
