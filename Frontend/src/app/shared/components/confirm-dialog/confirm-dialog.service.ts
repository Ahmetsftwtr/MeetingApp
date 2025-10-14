import { Injectable, signal, ViewContainerRef, ComponentRef, createComponent, EnvironmentInjector, inject } from '@angular/core';
import { ConfirmDialogComponent, ConfirmDialogData } from './confirm-dialog.component';

@Injectable({
  providedIn: 'root'
})
export class ConfirmDialogService {
  private environmentInjector = inject(EnvironmentInjector);
  private dialogComponentRef: ComponentRef<ConfirmDialogComponent> | null = null;
  private viewContainerRef: ViewContainerRef | null = null;

  setViewContainerRef(vcr: ViewContainerRef): void {
    this.viewContainerRef = vcr;
  }

  private ensureDialogComponent(): ConfirmDialogComponent {
    if (!this.dialogComponentRef && this.viewContainerRef) {
      this.dialogComponentRef = createComponent(ConfirmDialogComponent, {
        environmentInjector: this.environmentInjector,
        elementInjector: this.viewContainerRef.injector
      });
      this.viewContainerRef.insert(this.dialogComponentRef.hostView);
    }
    return this.dialogComponentRef!.instance;
  }

  confirm(title: string, message: string, confirmText: string = 'Onayla', cancelText: string = 'İptal'): Promise<boolean> {
    const dialog = this.ensureDialogComponent();
    return dialog.open({
      type: 'info',
      title,
      message,
      confirmText,
      cancelText
    });
  }

  warn(title: string, message: string, confirmText: string = 'Devam Et', cancelText: string = 'İptal'): Promise<boolean> {
    const dialog = this.ensureDialogComponent();
    return dialog.open({
      type: 'warning',
      title,
      message,
      confirmText,
      cancelText
    });
  }

  danger(title: string, message: string, confirmText: string = 'Sil', cancelText: string = 'İptal'): Promise<boolean> {
    const dialog = this.ensureDialogComponent();
    return dialog.open({
      type: 'danger',
      title,
      message,
      confirmText,
      cancelText
    });
  }
}
