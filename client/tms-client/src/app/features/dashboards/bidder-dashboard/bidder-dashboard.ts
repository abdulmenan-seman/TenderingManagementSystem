import { Component, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, Router, RouterLinkActive, RouterOutlet } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';
import { AuthService } from '../../../core/services/auth';

interface NavItem {
  label: string;
  icon: string;
  route: string;
  exact?: boolean;
}

@Component({
  selector: 'app-bidder-dashboard',
  standalone: true,
  imports: [
    CommonModule, 
    RouterLink, 
    RouterLinkActive,
    RouterOutlet,
    MatIconModule, 
    MatButtonModule, 
    MatMenuModule, 
    MatTooltipModule
  ],
  templateUrl: './bidder-dashboard.html',
  styleUrl: './bidder-dashboard.scss'
})
export class BidderDashboardComponent {
  private authService = inject(AuthService);

  isSidebarCollapsed = signal<boolean>(false);

  // Relative navigation routes for child outlets
  navItems: NavItem[] = [
    { label: 'Overview', icon: 'home', route: '/dashboard/bidder', exact: true },
    { label: 'Tenders', icon: 'article', route: '/dashboard/bidder/tenders' },
    { label: 'My Bids', icon: 'view_list', route: '/dashboard/bidder/my-bids' },
    { label: 'New Bid', icon: 'add_circle', route: '/dashboard/bidder/bids/new' },
    { label: 'Results', icon: 'emoji_events', route: '/dashboard/bidder/results' },
    { label: 'Support', icon: 'headset_mic', route: '/dashboard/bidder/support' }
  ];

  toggleSidebar(): void {
    this.isSidebarCollapsed.update(val => !val);
  }

  logout(): void {
    this.authService.logout();
  }
}