import { Routes } from '@angular/router';

export const routes: Routes = [
    {
        path: '',
        redirectTo: '/apply',
        pathMatch: 'full'
    },
    {
        path: 'apply',
        loadComponent: () => import('./apply/apply.component').then(c => c.ApplyComponent),
        title: 'Apply',
    },
    {
        path: 'questions',
        loadComponent: () => import('./questions/questions.component').then(c => c.QuestionsComponent),
        title: 'Questions',
    },
    {
        path: 'jobs',
        loadComponent: () => import('./jobs/jobs.component').then(c => c.JobsComponent),
        title: 'Jobs',
    },
    {
        path: 'jobs/:jobID',
        loadComponent: () => import('./jobDetail/jobDetail.component').then(c => c.JobDetailComponent),
        title: 'Jobs',
    },
];
