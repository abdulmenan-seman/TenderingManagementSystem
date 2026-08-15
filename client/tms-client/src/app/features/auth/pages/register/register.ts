import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { AuthService } from '../../../../core/services/auth';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule,
    MatInputModule,
    MatFormFieldModule,
    MatButtonModule,
    MatIconModule
  ],
  templateUrl: './register.html',
  styleUrl: './register.scss'
})
export class RegisterComponent {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);

  hidePassword = true;
  hideConfirmPassword = true;
  isLoading = false;
  errorMessage = '';

  registerForm = this.fb.group({
    // Account Credentials
    fullName: ['', [Validators.required, Validators.minLength(3)]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
    confirmPassword: ['', [Validators.required]],

    // Supplier Profile Credentials
    companyName: ['', [Validators.required, Validators.minLength(2)]],
    taxIdNumber: ['', [Validators.required]],
    businessLicenseNumber: ['', [Validators.required]],
    phoneNumber: ['', [Validators.required, Validators.pattern('^[0-9+ ]{9,15}$')]],
    address: ['', [Validators.required, Validators.minLength(5)]]
  }, { validators: this.passwordMatchValidator });

  private passwordMatchValidator(group: AbstractControl): ValidationErrors | null {
    const password = group.get('password')?.value;
    const confirmPassword = group.get('confirmPassword')?.value;
    return password === confirmPassword ? null : { passwordMismatch: true };
  }

  onSubmit(): void {
    if (this.registerForm.valid) {
      this.isLoading = true;
      this.errorMessage = '';

      const formValue = this.registerForm.value;

      const userData = {
        fullName: formValue.fullName!,
        email: formValue.email!,
        password: formValue.password!
      };

      const profileData = {
        companyName: formValue.companyName!,
        taxIdNumber: formValue.taxIdNumber!,
        businessLicenseNumber: formValue.businessLicenseNumber!,
        phoneNumber: formValue.phoneNumber!,
        address: formValue.address!
      };

      this.authService.registerAndSetupSupplier(userData, profileData).subscribe({
        next: () => {
          this.authService.navigateToDashboard();
        },
        error: (err) => {
          this.isLoading = false;
          if (err.status === 409) {
            this.errorMessage = 'A user with this email address already exists.';
          } else if (err.error?.detail) {
            this.errorMessage = err.error.detail;
          } else if (err.error?.title) {
            this.errorMessage = err.error.title;
          } else {
            this.errorMessage = 'Registration failed. Please review your company details and try again.';
          }
        }
      });
    }
  }
}