import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { guestGuard } from './core/guards/guest.guard';

export const routes: Routes = [
    {
        path: '',
        redirectTo: '/meetings',
        pathMatch: 'full'
    },
    {
        path: 'auth',
        canActivate: [guestGuard],
        children: [
            {
                path: '',
                redirectTo: 'login',
                pathMatch: 'full'
            },
            {
                path: 'login',
                loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent)
            },
            {
                path: 'register',
                loadComponent: () => import('./features/auth/register/register.component').then(m => m.RegisterComponent)
            }
        ]
    },
    {
        path: 'meetings',
        canActivate: [authGuard],
        children: [
            {
                path: '',
                redirectTo: 'list',
                pathMatch: 'full'
            },
            {
                path: 'list',
                loadComponent: () => import('./features/meetings/meeting-list/meeting-list.component').then(m => m.MeetingListComponent)
            },
            {
                path: 'create',
                loadComponent: () => import('./features/meetings/meeting-create/meeting-create.component').then(m => m.MeetingCreateComponent)
            },
            {
                path: ':id/edit',
                loadComponent: () => import('./features/meetings/meeting-edit/meeting-edit.component').then(m => m.MeetingEditComponent)
            },
            {
                path: ':id',
                loadComponent: () => import('./features/meetings/meeting-detail/meeting-detail.component').then(m => m.MeetingDetailComponent)
            }
        ]
    },
    {
        path: '**',
        redirectTo: '/meetings'
    }
];
