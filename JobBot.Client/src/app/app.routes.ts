import { Routes } from '@angular/router';

export const routes: Routes = [
    {
        path: '',
        loadComponent: () => import('./apply/apply.component').then(c => c.ApplyComponent),
        title: 'Apply',
    },
];
