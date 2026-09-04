import { Component, OnInit, inject, signal, viewChild } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ConfirmDialog } from '../../../shared/components/confirm-dialog/confirm-dialog';
import { QuoteService } from '../services/quote.service';

@Component({
  selector: 'app-quote-list',
  imports: [RouterLink, ConfirmDialog],
  templateUrl: './quote-list.html',
})
export class QuoteList implements OnInit {
  private readonly quoteService = inject(QuoteService);
  private readonly confirmDialog = viewChild.required(ConfirmDialog);

  readonly quotes = this.quoteService.quotes;
  readonly isLoading = signal(true);
  readonly errorMessage = signal<string | null>(null);

  private quoteIdPendingDelete: number | null = null;

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.quoteService.loadAll().subscribe({
      next: () => this.isLoading.set(false),
      error: () => {
        this.isLoading.set(false);
        this.errorMessage.set('Det gick inte att hämta citaten. Försök igen senare.');
      },
    });
  }

  askDelete(id: number, text: string): void {
    this.quoteIdPendingDelete = id;
    const preview = text.length > 60 ? `${text.slice(0, 60)}…` : text;
    this.confirmDialog().open('Ta bort citat', `Vill du ta bort "${preview}"? Det går inte att ångra.`);
  }

  confirmDelete(): void {
    if (this.quoteIdPendingDelete === null) {
      return;
    }

    const id = this.quoteIdPendingDelete;
    this.quoteIdPendingDelete = null;

    this.quoteService.delete(id).subscribe({
      error: () => this.errorMessage.set('Det gick inte att ta bort citatet. Försök igen.'),
    });
  }
}
