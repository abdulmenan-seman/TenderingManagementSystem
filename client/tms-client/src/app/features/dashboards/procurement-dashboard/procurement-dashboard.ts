import { Component, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive, Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';
import { AuthService } from '../../../core/services/auth';

interface ModuleCard {
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
  selector: 'app-procurement-dashboard',
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
  templateUrl: './procurement-dashboard.html',
  styleUrl: './procurement-dashboard.scss'
})
export class ProcurementDashboardComponent {
  private router = inject(Router);
   private authService = inject(AuthService);

  isSidebarCollapsed = signal<boolean>(false);

  navItems: NavItem[] = [
    { label: 'Dashboard', icon: 'home', route: '/dashboard/procurement' },
    { label: 'Tenders', icon: 'assignment', route: '/procurement/tenders' },
    { label: 'Bids', icon: 'inventory_2', route: '/procurement/bids' },
    { label: 'Evaluation', icon: 'person_search', route: '/procurement/evaluation' },
    { label: 'Awards', icon: 'work_history', route: '/procurement/awards' },
    { label: 'Reports', icon: 'show_chart', route: '/procurement/reports' },
    { label: 'Settings', icon: 'settings', route: '/procurement/settings' }
  ];

  primaryCards: ModuleCard[] = [
    {
      title: 'Tender Management',
      subtitle: 'Create & Manage Tenders',
      icon: 'campaign',
      bgGradient: 'bg-sky-100',
      iconColor: 'text-blue-600',
      route: '/procurement/tenders'
    },
    {
      title: 'Bid Administration',
      subtitle: 'Review Supplier Bids',
      icon: 'mark_email_read',
      bgGradient: 'bg-blue-100',
      iconColor: 'text-blue-700',
      route: '/procurement/bids'
    },
    {
      title: 'Evaluation Coordination',
      subtitle: 'Assign & Monitor Evaluations',
      icon: 'record_voice_over',
      bgGradient: 'bg-indigo-100',
      iconColor: 'text-indigo-700',
      route: '/procurement/evaluation'
    },
    {
      title: 'Award & Approval',
      subtitle: 'Approve Winning Bids',
      icon: 'stars',
      bgGradient: 'bg-amber-100',
      iconColor: 'text-amber-600',
      route: '/procurement/awards'
    },
    {
      title: 'Reports & Analytics',
      subtitle: 'Generate Insights',
      icon: 'analytics',
      bgGradient: 'bg-emerald-100',
      iconColor: 'text-emerald-700',
      route: '/procurement/reports'
    },
    {
      title: 'Documents & Records',
      subtitle: 'Manage Tender Files',
      icon: 'folder_shared',
      bgGradient: 'bg-amber-100',
      iconColor: 'text-yellow-700',
      route: '/procurement/documents'
    }
  ];

  secondaryBanners: ModuleCard[] = [
    {
      title: 'Notifications & Messages',
      subtitle: 'Send Alerts & Announcements',
      icon: 'speaker_notes',
      bgGradient: 'bg-blue-100',
      iconColor: 'text-blue-600',
      route: '/procurement/notifications'
    },
    {
      title: 'Compliance & Integrity',
      subtitle: 'Ensure Rules & Transparency',
      icon: 'verified_user',
      bgGradient: 'bg-emerald-100',
      iconColor: 'text-emerald-600',
      route: '/procurement/compliance'
    }
  ];

  toggleSidebar(): void {
    this.isSidebarCollapsed.update(val => !val);
  }

  logout(): void {
    this.authService.logout();
  }
}