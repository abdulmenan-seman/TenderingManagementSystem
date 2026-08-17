import { Routes } from '@angular/router';
import { roleGuard, unauthGuard  } from './core/guards/auth-guard';
import { UserManagementComponent } from './features/admin/components/user-management/user-management';


export const routes: Routes = [
  // Home route
  { path: 'app-home', loadComponent: () => import('./features/home/home.component').then(m => m.HomeComponent) },
  { path: '', redirectTo: 'app-home', pathMatch: 'full' },
  { path: 'auth/login',
     canActivate: [unauthGuard],
      loadComponent: () => import('./features/auth/pages/login/login').then(m => m.LoginComponent) },
  { path: 'auth/register', canActivate: [unauthGuard], loadComponent: () => import('./features/auth/pages/register/register').then(m => m.RegisterComponent) },
  // Protected Admin Route
  // 1. Admin Dashboard
  {
    path: 'dashboard/admin',
    canActivate: [roleGuard],
    data: { roles: ['Admin'] },
    loadComponent: () => import('./features/dashboards/admin-dashboard/admin-dashboard').then(m => m.AdminDashboardComponent),
    children: [
      {
        path: 'users',
        component: UserManagementComponent,
        title: 'User Management'
      }
    ]
  },

  // 2. Bidder / Supplier Dashboard
  {
    path: 'dashboard/bidder',
    canActivate: [roleGuard],
    data: { roles: ['Bidder'] },
    loadComponent: () => import('./features/dashboards/bidder-dashboard/bidder-dashboard').then(m => m.BidderDashboard)
  },

  // // 3. Procurement / Tender Officer Dashboard
  // {
  //   path: 'dashboard/procurement',
  //   canActivate: [roleGuard],
  //   data: { roles: ['TenderOfficer', 'Admin'] }, // Admin can also view if needed
  //   loadComponent: () => import('./features/dashboards/procurement-dashboard/procurement-dashboard').then(m => m.ProcurementDashboardComponent)
  // },

  // // 4. Evaluation Committee Dashboard
  // {
  //   path: 'dashboard/evaluation',
  //   canActivate: [roleGuard],
  //   data: { roles: ['Evaluator', 'Admin'] },
  //   loadComponent: () => import('./features/dashboard/evaluation/evaluation-dashboard.component').then(m => m.EvaluationDashboardComponent)
  // },
];