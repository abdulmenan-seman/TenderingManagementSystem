import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, FormArray, FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';
import { UserManagementService } from '../../../../core/services/user-management';
import { UserListItem, LoginActivityLog, CreateUserDto, UpdateUserDto } from '../../../../core/models/user-management.model';

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

  // Core Data Signals
  users = signal<UserListItem[]>([]);
  totalUsers = signal<number>(0);
  isLoading = signal<boolean>(false);
  isSubmitting = signal<boolean>(false); // Lock state to prevent 429 rate-limiting
  activeTab = signal<'users' | 'activity'>('users');
  
  // Modal & Selection Signals
  isUserModalOpen = signal<boolean>(false);
  isPasswordModalOpen = signal<boolean>(false);
  selectedUser = signal<UserListItem | null>(null);
  activityLogs = signal<LoginActivityLog[]>([]);

  // Restricted Internal Roles
  availableRoles: string[] = ['Admin', 'TenderOfficer', 'Evaluator'];

  // Filters & Pagination State
  searchTerm = signal<string>('');
  selectedRole = signal<string>('');
  currentPage = signal<number>(1);
  pageSize = signal<number>(10);

  // Reactive Forms
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
      roles: this.fb.array([], [Validators.required]),
      isActive: [true],
      password: ['']
    });

    this.passwordForm = this.fb.group({
      newPassword: ['', [Validators.required, Validators.minLength(8)]],
      mustChangePasswordOnLogin: [true]
    });
  }

  get rolesFormArray(): FormArray {
    return this.userForm.get('roles') as FormArray;
  }

  isRoleSelected(role: string): boolean {
    return this.rolesFormArray.controls.some(ctrl => ctrl.value === role);
  }

  onRoleCheckboxChange(event: Event, role: string): void {
    const checked = (event.target as HTMLInputElement).checked;
    if (checked) {
      if (!this.isRoleSelected(role)) {
        this.rolesFormArray.push(new FormControl(role));
      }
    } else {
      const index = this.rolesFormArray.controls.findIndex(ctrl => ctrl.value === role);
      if (index !== -1) {
        this.rolesFormArray.removeAt(index);
      }
    }
    this.rolesFormArray.markAsTouched();
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
    this.rolesFormArray.clear();
    this.userForm.reset({ isActive: true });
    
    this.userForm.get('password')?.setValidators([Validators.required, Validators.minLength(8)]);
    this.userForm.get('password')?.updateValueAndValidity();

    this.isUserModalOpen.set(true);
  }

  openEditModal(user: UserListItem): void {
    this.selectedUser.set(user);
    this.rolesFormArray.clear();

    this.userForm.get('password')?.clearValidators();
    this.userForm.get('password')?.updateValueAndValidity();
    
    user.roles.forEach(r => {
      if (this.availableRoles.includes(r)) {
        this.rolesFormArray.push(new FormControl(r));
      }
    });

    this.userForm.patchValue({
      fullName: user.fullName,
      email: user.email,
      isActive: user.isActive,
      password: ''
    });
    this.isUserModalOpen.set(true);
  }

  openResetPasswordModal(user: UserListItem): void {
    this.selectedUser.set(user);
    this.passwordForm.reset({ mustChangePasswordOnLogin: true });
    this.isPasswordModalOpen.set(true);
  }

  saveUser(): void {
    if (this.userForm.invalid || this.isSubmitting()) {
      this.userForm.markAllAsTouched();
      return;
    }

    const rawValue = this.userForm.value;
    const selectedUser = this.selectedUser();

    // Extract primary role string from FormArray (C# API expects a single string)
    const primaryRole: string = Array.isArray(rawValue.roles) && rawValue.roles.length > 0
      ? rawValue.roles[0]
      : 'Evaluator';

    this.isSubmitting.set(true);

    if (selectedUser) {
      const updatePayload: UpdateUserDto = {
        fullName: rawValue.fullName,
        email: rawValue.email,
        role: primaryRole,
        isActive: rawValue.isActive
      };

      this.userService.updateUser(selectedUser.id, updatePayload).subscribe({
        next: () => {
          this.isSubmitting.set(false);
          this.isUserModalOpen.set(false);
          this.loadUsers();
        },
        error: (err) => {
          this.isSubmitting.set(false);
          console.error('Failed to update staff:', err);
        }
      });
    } else {
      const createPayload: CreateUserDto = {
        fullName: rawValue.fullName,
        email: rawValue.email,
        role: primaryRole,
        password: rawValue.password
      };

      this.userService.createUser(createPayload).subscribe({
        next: () => {
          this.isSubmitting.set(false);
          this.isUserModalOpen.set(false);
          this.loadUsers();
        },
        error: (err) => {
          this.isSubmitting.set(false);
          console.error('Failed to create staff:', err);
        }
      });
    }
  }

  confirmResetPassword(): void {
    if (this.passwordForm.invalid || !this.selectedUser() || this.isSubmitting()) return;

    this.isSubmitting.set(true);
    this.userService.resetPassword(this.selectedUser()!.id, this.passwordForm.value).subscribe({
      next: () => {
        this.isSubmitting.set(false);
        this.isPasswordModalOpen.set(false);
      },
      error: (err) => {
        this.isSubmitting.set(false);
        console.error('Failed to reset password:', err);
      }
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