import { Component, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, Router, RouterLinkActive } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';
import { AuthService } from '../../../core/services/auth';

interface DashboardCard {
  title: string;
  subtitle: string;
  icon: string;
  bgGradient: string;
  iconColor: string;
  route: string;
}

interface NavItem {
  label: string;
  icon: string;
  route: string;
}

@Component({
  selector: 'app-evaluation-dashboard',
  standalone: true,
  imports: [
    CommonModule, 
    RouterLink, 
    RouterLinkActive,
    MatIconModule, 
    MatButtonModule, 
    MatMenuModule, 
    MatTooltipModule
  ],
  templateUrl: './evaluation-dashboard.html',
  styleUrl: './evaluation-dashboard.scss'
})
export class EvaluationDashboardComponent {
  private router = inject(Router);
  private authService = inject(AuthService);

  isSidebarCollapsed = signal<boolean>(false);

  // Evaluator Specific Navigation Items
  navItems: NavItem[] = [
    { label: 'Dashboard', icon: 'home', route: '/dashboard/evaluator' },
    { label: 'Assigned Tenders', icon: 'folder_open', route: '/evaluator/assigned-tenders' },
    { label: 'Bid Evaluation', icon: 'fact_check', route: '/evaluator/bid-evaluation' },
    { label: 'Reports', icon: 'assessment', route: '/evaluator/reports' },
    { label: 'Notifications', icon: 'notifications', route: '/evaluator/notifications' },
    { label: 'Compliance', icon: 'gavel', route: '/evaluator/compliance' }
  ];

  // Core Functional Modules Card Array
  dashboardCards: DashboardCard[] = [
    {
      title: 'Assigned Tenders',
      subtitle: 'View Assigned Bids & Documents',
      icon: 'folder_open',
      bgGradient: 'bg-sky-100',
      iconColor: 'text-blue-600',
      route: '/evaluator/assigned-tenders'
    },
    {
      title: 'Bid Evaluation',
      subtitle: 'Score & Review Supplier Proposals',
      icon: 'fact_check',
      bgGradient: 'bg-blue-100',
      iconColor: 'text-blue-700',
      route: '/evaluator/bid-evaluation'
    },
    {
      title: 'Reports',
      subtitle: 'Submit Final Evaluation Reports',
      icon: 'assessment',
      bgGradient: 'bg-indigo-100',
      iconColor: 'text-indigo-700',
      route: '/evaluator/reports'
    },
    {
      title: 'Notifications',
      subtitle: 'Alerts, Assignments & Deadlines',
      icon: 'notifications_active',
      bgGradient: 'bg-amber-100',
      iconColor: 'text-amber-600',
      route: '/evaluator/notifications'
    },
    {
      title: 'Compliance & Ethics',
      subtitle: 'Ensure Fair Evaluation & Audit Trail',
      icon: 'security',
      bgGradient: 'bg-emerald-100',
      iconColor: 'text-emerald-700',
      route: '/evaluator/compliance'
    }
  ];

  toggleSidebar(): void {
    this.isSidebarCollapsed.update(val => !val);
  }

  logout(): void {
    this.authService.logout();
  }
}