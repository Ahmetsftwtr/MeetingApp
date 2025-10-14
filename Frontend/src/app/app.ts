import { Component, inject, ViewContainerRef, AfterViewInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { HeaderComponent } from './shared/layout/header/header.component';
import { SidebarComponent } from './shared/layout/sidebar/sidebar.component';
import { AuthService } from './core/services/auth.service';
import { AsyncPipe } from '@angular/common';
import { ConfirmDialogService } from './shared/components/confirm-dialog/confirm-dialog.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, HeaderComponent, SidebarComponent, AsyncPipe],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App implements AfterViewInit {
  private authService = inject(AuthService);
  private confirmDialogService = inject(ConfirmDialogService);
  private viewContainerRef = inject(ViewContainerRef);

  isAuthenticated$ = this.authService.isAuthenticated$;

  ngAfterViewInit(): void {
    this.confirmDialogService.setViewContainerRef(this.viewContainerRef);
  }
}
