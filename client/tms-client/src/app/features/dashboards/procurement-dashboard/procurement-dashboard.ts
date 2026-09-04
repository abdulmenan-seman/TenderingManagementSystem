import { Component, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
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
  selector: 'app-procurement-dashboard',
  standalone: true,
  imports: [
    CommonModule, 
    RouterOutlet,
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
  private authService = inject(AuthService);
  currentUser = this.authService.currentUser;

  isSidebarCollapsed = signal<boolean>(false);

  navItems: NavItem[] = [
    { label: 'Dashboard', icon: 'home', route: '/dashboard/procurement/overview', exact: true },
    { label: 'Tenders', icon: 'assignment', route: '/dashboard/procurement/tenders' },
    { label: 'Bids', icon: 'inventory_2', route: '/dashboard/procurement/bids' },
    { label: 'Evaluation', icon: 'person_search', route: '/dashboard/procurement/evaluation' },
    { label: 'Awards', icon: 'work_history', route: '/dashboard/procurement/awards' },
    { label: 'Reports', icon: 'show_chart', route: '/dashboard/procurement/reports' },
    { label: 'Settings', icon: 'settings', route: '/dashboard/procurement/settings' }
  ];

  toggleSidebar(): void {
    this.isSidebarCollapsed.update(val => !val);
  }

  logout(): void {
    this.authService.logout();
  }
}