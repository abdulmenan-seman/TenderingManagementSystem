import { Component, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router'; // <-- Added RouterOutlet & RouterLinkActive
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';
import { AuthService } from '../../../core/services/auth';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    RouterLinkActive, // <-- Added
    RouterOutlet,     // <-- Added
    MatIconModule,
    MatButtonModule,
    MatMenuModule,
    MatTooltipModule
  ],
  templateUrl: './admin-dashboard.html',
  styleUrl: './admin-dashboard.scss'
})
export class AdminDashboardComponent {
  authService = inject(AuthService);
  private router = inject(Router);

  isCollapsed = signal<boolean>(false);
  isMobileOpen = signal<boolean>(false);

  navItems = [
    { label: 'Dashboard', icon: 'dashboard', route: '/dashboard/admin' },
    { label: 'Tenders', icon: 'gavel', route: '/dashboard/admin/tenders' },
    { label: 'Bids', icon: 'visibility', route: '/dashboard/admin/bids' },
    { label: 'Users', icon: 'group', route: '/dashboard/admin/users' },
    { label: 'Reports', icon: 'bar_chart', route: '/dashboard/admin/reports' },
    { label: 'Settings', icon: 'settings', route: '/dashboard/admin/settings' }
  ];

  toggleSidebar(): void {
    this.isCollapsed.update(v => !v);
  }

  toggleMobileSidebar(): void {
    this.isMobileOpen.update(v => !v);
  }

  logout(): void {
    this.authService.logout();
  }
}