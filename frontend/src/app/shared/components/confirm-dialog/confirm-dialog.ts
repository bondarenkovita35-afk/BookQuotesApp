import { Component, ElementRef, output, viewChild } from '@angular/core';

@Component({
  selector: 'app-confirm-dialog',
  templateUrl: './confirm-dialog.html',
  styleUrl: './confirm-dialog.scss',
})
export class ConfirmDialog {
  private readonly dialog = viewChild.required<ElementRef<HTMLDialogElement>>('dialog');

  readonly confirmed = output<void>();

  title = '';
  message = '';

  open(title: string, message: string): void {
    this.title = title;
    this.message = message;
    this.dialog().nativeElement.showModal();
  }

  close(): void {
    this.dialog().nativeElement.close();
  }

  confirm(): void {
    this.close();
    this.confirmed.emit();
  }
}
