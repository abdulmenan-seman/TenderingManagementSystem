import { Injectable, signal, computed, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, switchMap, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { 
  LoginRequestDto, 
  AuthResponseDto, 
  RegisterUserRequestDto, 
  UserResponseDto,
  CreateSupplierProfileRequestDto,
  SupplierProfileResponseDto
} from '../models/auth.models';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);

  private readonly AUTH_URL = `${environment.apiUrl}/auth`;
  private readonly SUPPLIER_URL = `${environment.apiUrl}/supplier-profiles`;


  // Reactive state using Angular Signals
  currentUser = signal<AuthResponseDto | null>(null);
  supplierProfile = signal<SupplierProfileResponseDto | null>(null);
  isAuthenticated = computed(() => !!this.currentUser());

  login(credentials: LoginRequestDto): Observable<AuthResponseDto> {
    return this.http.post<AuthResponseDto>(`${this.AUTH_URL}/login`, credentials).pipe(
      tap((response) => {
        this.currentUser.set(response);
      })
    );
  }

  registerUser(data: RegisterUserRequestDto): Observable<UserResponseDto> {
    return this.http.post<UserResponseDto>(`${this.AUTH_URL}/register`, data);
  }

  createSupplierProfile(profile: CreateSupplierProfileRequestDto): Observable<SupplierProfileResponseDto> {
    return this.http.post<SupplierProfileResponseDto>(this.SUPPLIER_URL, profile).pipe(
      tap((res) => this.supplierProfile.set(res))
    );
  }

  /**
   * Orchestrates full Bidder/Supplier registration flow:
   * 1. Register User (User Account)
   * 2. Authenticate User (Get Auth Token & UserId)
   * 3. Create Supplier Profile bound to UserId
   */
  registerAndSetupSupplier(
    userData: { fullName: string; email: string; password: string },
    profileData: Omit<CreateSupplierProfileRequestDto, 'userId'>
  ): Observable<SupplierProfileResponseDto> {
    return this.registerUser(userData).pipe(
      switchMap(() => this.login({ email: userData.email, password: userData.password })),
      switchMap((authRes) => {
        const fullProfilePayload: CreateSupplierProfileRequestDto = {
          userId: authRes.userId,
          ...profileData
        };
        return this.createSupplierProfile(fullProfilePayload);
      })
    );
  }

  logout(): void {
    this.currentUser.set(null);
    this.supplierProfile.set(null);
    this.router.navigate(['/auth/login']);
  }

  navigateToDashboard(): void {
    const roles = this.currentUser()?.roles || [];
    if (roles.includes('Admin')) {
      this.router.navigate(['/dashboard/admin']);
    } else if (roles.includes('TenderOfficer')) {
      this.router.navigate(['/dashboard/procurement']);
    } else if (roles.includes('Evaluator')) {
      this.router.navigate(['/dashboard/evaluation']);
    } else if (roles.includes('Bidder')) {
      this.router.navigate(['/dashboard/bidder']);
    } else {
      this.router.navigate(['/']);
    }
  }
}