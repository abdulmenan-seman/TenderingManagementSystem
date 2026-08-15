import { Routes } from '@angular/router';

export const routes: Routes = [
  // Home route
  { path: 'app-home', loadComponent: () => import('./features/home/home.component').then(m => m.HomeComponent) },
  { path: '', redirectTo: 'app-home', pathMatch: 'full' },
  { path: 'auth/login', loadComponent: () => import('./features/auth/pages/login/login').then(m => m.LoginComponent) },
  { path: 'auth/register', loadComponent: () => import('./features/auth/pages/register/register').then(m => m.RegisterComponent) },
];