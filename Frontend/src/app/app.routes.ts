import { Routes } from '@angular/router';
import { LoginComponent } from './features/auth/login/login.component';
import { RegisterComponent } from './features/auth/register/register.component';
import { MeetingDetailComponent } from './features/meetings/meeting-detail/meeting-detail.component';
import { MeetingCreateComponent } from './features/meetings/meeting-create/meeting-create.component';
import { MeetingListComponent } from './features/meetings/meeting-list/meeting-list.component';
import { MeetingEditComponent } from './features/meetings/meeting-edit/meeting-edit.component';

export const routes: Routes = [
    {
        path: '',
        redirectTo: 'auth/login',
        pathMatch: 'full'
    },
    {
        path: 'auth/login',
        component: LoginComponent
    },
    {
        path: 'auth/register',
        component: RegisterComponent
    },
    {
        path: 'meetings',
        children: [
            {
                path: '',
                redirectTo: 'list',
                pathMatch: 'full'
            },
            {
                path: 'list',
                component: MeetingListComponent
            },
            {
                path: 'create',
                component: MeetingCreateComponent
            },
            {
                path: ':id/edit',
                component: MeetingEditComponent
            },
            {
                path: ':id',
                component: MeetingDetailComponent
            },
            
        ]
    }
];
