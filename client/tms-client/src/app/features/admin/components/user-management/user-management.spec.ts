import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';
import { UserManagementService } from '../../../../core/services/user-management';
import { UserListItem, LoginActivityLog } from '../../../../core/models/user-management.model';

@Component({
  selector: 'app-user-management',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatIconModule,
    MatButtonModule,
    MatMenuModule,
    MatTooltipModule
  ],
  templateUrl: './user-management.html'
})
export class UserManagementComponent implements OnInit {
  protected readonly Math = Math;
  private userService = inject(UserManagementService);
  private fb = inject(FormBuilder);

  users = signal<UserListItem[]>([]);
  totalUsers = signal<number>(0);
  isLoading = signal<boolean>(false);
  activeTab = signal<'users' | 'activity'>('users');
  
  isUserModalOpen = signal<boolean>(false);
  isPasswordModalOpen = signal<boolean>(false);
  selectedUser = signal<UserListItem | null>(null);
  activityLogs = signal<LoginActivityLog[]>([]);

  availableRoles: string[] = ['Admin', 'TenderOfficer', 'Evaluator'];

  searchTerm = signal<string>('');
  selectedRole = signal<string>('');
  currentPage = signal<number>(1);
  pageSize = signal<number>(10);

  userForm!: FormGroup;
  passwordForm!: FormGroup;

  ngOnInit(): void {
    this.initForms();
    this.loadUsers();
  }

  private initForms(): void {
    this.userForm = this.fb.group({
      fullName: ['', [Validators.required, Validators.minLength(3)]],
      email: ['', [Validators.required, Validators.email]],
      password: [''], // Required only for new users
      role: ['TenderOfficer', [Validators.required]],
      isActive: [true]
    });

    this.passwordForm = this.fb.group({
      newPassword: ['', [Validators.required, Validators.minLength(8)]],
      mustChangePasswordOnLogin: [true]
    });
  }

  getDisplayedCountEnd(): number {
    return Math.min(this.currentPage() * this.pageSize(), this.totalUsers());
  }

  loadUsers(): void {
    this.isLoading.set(true);
    this.userService.getUsers({
      searchTerm: this.searchTerm(),
      role: this.selectedRole(),
      pageNumber: this.currentPage(),
      pageSize: this.pageSize()
    }).subscribe({
      next: (res) => {
        this.users.set(res.items);
        this.totalUsers.set(res.totalCount);
        this.isLoading.set(false);
      },
      error: () => this.isLoading.set(false)
    });
  }

  loadActivityLogs(): void {
    this.userService.getLoginActivityLogs().subscribe({
      next: (logs) => this.activityLogs.set(logs)
    });
  }

  openCreateModal(): void {
    this.selectedUser.set(null);
    this.userForm.reset({ role: 'TenderOfficer', isActive: true });
    this.userForm.get('password')?.setValidators([Validators.required, Validators.minLength(8)]);
    this.userForm.get('password')?.updateValueAndValidity();
    this.isUserModalOpen.set(true);
  }

  openEditModal(user: UserListItem): void {
    this.selectedUser.set(user);
    this.userForm.get('password')?.clearValidators();
    this.userForm.get('password')?.updateValueAndValidity();

    this.userForm.patchValue({
      fullName: user.fullName,
      email: user.email,
      role: user.roles[0] || 'TenderOfficer',
      isActive: user.isActive
    });
    this.isUserModalOpen.set(true);
  }

  openResetPasswordModal(user: UserListItem): void {
    this.selectedUser.set(user);
    this.passwordForm.reset({ mustChangePasswordOnLogin: true });
    this.isPasswordModalOpen.set(true);
  }

  saveUser(): void {
    if (this.userForm.invalid) {
      this.userForm.markAllAsTouched();
      return;
    }

    const payload = this.userForm.value;
    const user = this.selectedUser();

    if (user) {
      this.userService.updateUser(user.id, payload).subscribe({
        next: () => {
          this.isUserModalOpen.set(false);
          this.loadUsers();
        }
      });
    } else {
      this.userService.createUser(payload).subscribe({
        next: () => {
          this.isUserModalOpen.set(false);
          this.loadUsers();
        }
      });
    }
  }

  confirmResetPassword(): void {
    if (this.passwordForm.invalid || !this.selectedUser()) return;

    this.userService.resetPassword(this.selectedUser()!.id, this.passwordForm.value).subscribe({
      next: () => this.isPasswordModalOpen.set(false)
    });
  }

  toggleUserStatus(user: UserListItem): void {
    this.userService.toggleUserStatus(user.id, !user.isActive).subscribe({
      next: () => this.loadUsers()
    });
  }

  onSearch(event: Event): void {
    const val = (event.target as HTMLInputElement).value;
    this.searchTerm.set(val);
    this.currentPage.set(1);
    this.loadUsers();
  }

  onRoleFilter(role: string): void {
    this.selectedRole.set(role);
    this.currentPage.set(1);
    this.loadUsers();
  }

  changePage(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage.set(page);
      this.loadUsers();
    }
  }

  get totalPages(): number {
    return Math.ceil(this.totalUsers() / this.pageSize());
  }

  switchTab(tab: 'users' | 'activity'): void {
    this.activeTab.set(tab);
    if (tab === 'activity') this.loadActivityLogs();
  }
}