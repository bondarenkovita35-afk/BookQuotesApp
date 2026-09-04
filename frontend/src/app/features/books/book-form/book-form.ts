import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { BookService } from '../services/book.service';

@Component({
  selector: 'app-book-form',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './book-form.html',
})
export class BookForm {
  private readonly fb = inject(FormBuilder);
  private readonly bookService = inject(BookService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  private readonly bookId = this.route.snapshot.paramMap.get('id');
  readonly isEditMode = this.bookId !== null;

  readonly form = this.fb.nonNullable.group({
    title: ['', [Validators.required, Validators.maxLength(200)]],
    author: ['', [Validators.required, Validators.maxLength(150)]],
    publishedDate: ['', [Validators.required]],
  });

  readonly isSubmitting = signal(false);
  readonly isLoading = signal(false);
  readonly errorMessage = signal<string | null>(null);

  constructor() {
    if (this.bookId) {
      this.isLoading.set(true);
      this.bookService.getById(Number(this.bookId)).subscribe({
        next: (book) => {
          this.form.setValue({ title: book.title, author: book.author, publishedDate: book.publishedDate });
          this.isLoading.set(false);
        },
        error: () => {
          this.isLoading.set(false);
          this.errorMessage.set('Boken kunde inte hämtas.');
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

    const request = this.form.getRawValue();
    const operation = this.isEditMode
      ? this.bookService.update(Number(this.bookId), request)
      : this.bookService.create(request);

    operation.subscribe({
      next: () => this.router.navigateByUrl('/books'),
      error: (error: HttpErrorResponse) => {
        this.isSubmitting.set(false);
        this.errorMessage.set(error.error?.message ?? 'Ett oväntat fel har uppstått. Försök igen.');
      },
    });
  }
}
