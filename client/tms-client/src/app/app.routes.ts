import { Routes } from '@angular/router';
import { roleGuard, authGuard, guestGuard } from './core/guards/auth-guard';
import { UserManagementComponent } from './features/admin/components/user-management/user-management';

export const routes: Routes = [
  // Home route
  { 
    path: 'app-home', 
    loadComponent: () => import('./features/home/home.component').then(m => m.HomeComponent) 
  },
  { 
    path: '', 
    redirectTo: 'app-home', 
    pathMatch: 'full' 
  },

  // Auth routes (protected by guestGuard so logged in users are redirected to dashboard)
  { 
    path: 'auth/login',
    canActivate: [guestGuard],
    loadComponent: () => import('./features/auth/pages/login/login').then(m => m.LoginComponent) 
  },
  { 
    path: 'auth/register', 
    canActivate: [guestGuard], 
    loadComponent: () => import('./features/auth/pages/register/register').then(m => m.RegisterComponent) 
  },

  // 1. Admin Dashboard
  {
    path: 'dashboard/admin',
    canActivate: [authGuard, roleGuard],
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
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Bidder'] },
    loadComponent: () => import('./features/dashboards/bidder-dashboard/bidder-dashboard').then(m => m.BidderDashboardComponent),
    // Child routes for bidder dashboard are defined within the BidderDashboardComponent using RouterOutlet
    children: [
      {
        path: 'tenders',
        title: 'Available Tenders',
        loadComponent: () => import('./features/bidder/Tenders/tenders.component').then(m => m.TendersComponent)
      },
    ]
    
  },

  // 3. Procurement / Tender Officer Dashboard
  {
    path: 'dashboard/procurement',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['TenderOfficer', 'Admin'] },
    loadComponent: () => import('./features/dashboards/procurement-dashboard/procurement-dashboard').then(m => m.ProcurementDashboardComponent),
    children: [
      {
        path: '',
        redirectTo: 'overview',
        pathMatch: 'full'
      },
      {
        path: 'overview',
        title: 'Procurement Overview',
        loadComponent: () => import('./features/procurement/components/procurement-overview/procurement-overview').then(m => m.ProcurementOverviewComponent)
      },
      {
        path: 'tenders',
        title: 'Tender Management',
        loadComponent: () => import('./features/tender/components/tender-management/tender-management').then(m => m.TenderManagementComponent)
      }
    ]
  },

  // 4. Evaluator Dashboard
  {
    path: 'dashboard/evaluator',
    canActivate: [authGuard, roleGuard],
    data: { roles: ['Evaluator', 'Admin'] },
    loadComponent: () => import('./features/dashboards/evaluation-dashboard/evaluation-dashboard').then(m => m.EvaluationDashboardComponent)
  }
];