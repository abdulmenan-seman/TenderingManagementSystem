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
  selector: 'app-bidder-dashboard',
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
  templateUrl: './bidder-dashboard.html',
  styleUrl: './bidder-dashboard.scss'
})
export class BidderDashboardComponent {
  private router = inject(Router);
  private authService = inject(AuthService);

  isSidebarCollapsed = signal<boolean>(false);

  navItems: NavItem[] = [
    { label: 'Dashboard', icon: 'home', route: '/dashboard/bidder' },
    { label: 'Tenders', icon: 'article', route: '/tenders' },
    { label: 'My Bids', icon: 'view_list', route: '/my-bids' },
    { label: 'Bids', icon: 'add_circle', route: '/bids/new' },
    { label: 'Results', icon: 'info', route: '/results' },
    { label: 'Support', icon: 'settings', route: '/support' }
  ];

  dashboardCards: DashboardCard[] = [
    {
      title: 'My Profile',
      subtitle: 'Manage Account Info',
      icon: 'contact_mail',
      bgGradient: 'bg-sky-100',
      iconColor: 'text-blue-600',
      route: '/profile'
    },
    {
      title: 'Active Tenders',
      subtitle: 'Browse & Apply for Tenders',
      icon: 'find_in_page',
      bgGradient: 'bg-blue-100',
      iconColor: 'text-blue-700',
      route: '/tenders'
    },
    {
      title: 'My Bids',
      subtitle: 'View & Track Submissions',
      icon: 'fact_check',
      bgGradient: 'bg-slate-100',
      iconColor: 'text-blue-800',
      route: '/my-bids'
    },
    {
      title: 'Alerts & Messages',
      subtitle: 'Tender Updates & Notices',
      icon: 'mark_email_unread',
      bgGradient: 'bg-amber-100',
      iconColor: 'text-amber-600',
      route: '/messages'
    },
    {
      title: 'Bid Status',
      subtitle: 'Check Bid Progress',
      icon: 'assignment_turned_in',
      bgGradient: 'bg-emerald-100',
      iconColor: 'text-emerald-700',
      route: '/status'
    },
    {
      title: 'Results & History',
      subtitle: 'View Awards & Reports',
      icon: 'emoji_events',
      bgGradient: 'bg-yellow-100',
      iconColor: 'text-amber-500',
      route: '/results'
    }
  ];

  toggleSidebar(): void {
    this.isSidebarCollapsed.update(val => !val);
  }
logout(): void {
    this.authService.logout();
  }
}