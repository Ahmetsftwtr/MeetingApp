import { Component, signal } from '@angular/core';
import { IconComponent } from '../icon/icon.component';

export interface ConfirmDialogData {
  title: string;
  message: string;
  confirmText?: string;
  cancelText?: string;
  type?: 'danger' | 'warning' | 'info';
}

@Component({
  selector: 'app-confirm-dialog',
  imports: [IconComponent],
  templateUrl: './confirm-dialog.component.html',
  styleUrl: './confirm-dialog.component.scss'
})
export class ConfirmDialogComponent {
  isOpen = signal(false);
  data = signal<ConfirmDialogData | null>(null);
  private resolvePromise?: (value: boolean) => void;

  open(data: ConfirmDialogData): Promise<boolean> {
    this.data.set({
      confirmText: 'Onayla',
      cancelText: 'İptal',
      type: 'info',
      ...data
    });
    this.isOpen.set(true);

    return new Promise<boolean>((resolve) => {
      this.resolvePromise = resolve;
    });
  }

  close(result: boolean): void {
    this.isOpen.set(false);
    if (this.resolvePromise) {
      this.resolvePromise(result);
      this.resolvePromise = undefined;
    }
  }

  getIcon(): 'warning' | 'error' | 'info' {
    const type = this.data()?.type || 'info';
    if (type === 'danger') return 'error';
    if (type === 'warning') return 'warning';
    return 'info';
  }
}
