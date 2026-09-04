import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { QuoteService } from '../services/quote.service';

@Component({
  selector: 'app-quote-form',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './quote-form.html',
})
export class QuoteForm {
  private readonly fb = inject(FormBuilder);
  private readonly quoteService = inject(QuoteService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  private readonly quoteId = this.route.snapshot.paramMap.get('id');
  readonly isEditMode = this.quoteId !== null;

  readonly form = this.fb.nonNullable.group({
    text: ['', [Validators.required, Validators.maxLength(1000)]],
    author: [''],
  });

  readonly isSubmitting = signal(false);
  readonly isLoading = signal(false);
  readonly errorMessage = signal<string | null>(null);

  constructor() {
    if (this.quoteId) {
      this.isLoading.set(true);
      this.quoteService.getById(Number(this.quoteId)).subscribe({
        next: (quote) => {
          this.form.setValue({ text: quote.text, author: quote.author ?? '' });
          this.isLoading.set(false);
        },
        error: () => {
          this.isLoading.set(false);
          this.errorMessage.set('Citatet kunde inte hämtas.');
        },
      });
    }
  }

  submit(): void {
    if (this.form.invalid || this.isSubmitting()) {
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    const raw = this.form.getRawValue();
    const request = { text: raw.text, author: raw.author.trim() === '' ? null : raw.author };
    const operation = this.isEditMode
      ? this.quoteService.update(Number(this.quoteId), request)
      : this.quoteService.create(request);

    operation.subscribe({
      next: () => this.router.navigateByUrl('/quotes'),
      error: (error: HttpErrorResponse) => {
        this.isSubmitting.set(false);
        this.errorMessage.set(error.error?.message ?? 'Ett oväntat fel har uppstått. Försök igen.');
      },
    });
  }
}
