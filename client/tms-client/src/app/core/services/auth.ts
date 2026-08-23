import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap, catchError, throwError, of } from 'rxjs';
import { environment } from '../../../environments/environment';
import { 
  User, 
  TokenResponse, 
  LoginRequest, 
  RegisterBidderRequest, 
  RefreshTokenRequest 
} from '../models/auth.models';

export interface RegisterSupplierUserData {
  fullName: string;
  email: string;
  password: string;
}

export interface RegisterSupplierProfileData {
  companyName: string;
  contactPerson: string;
  taxIdNumber: string;
  businessLicenseNumber: string;
  phoneNumber: string;
  address: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly apiUrl = `${environment.apiUrl || 'http://localhost:5000/api/v1'}/auth`;

  // 1. In-Memory Token & User State using Angular 21 Signals
  private readonly accessTokenSignal = signal<string | null>(null);
  readonly currentUser = signal<User | null>(this.getStoredUser());

  // 2. Computed Reactivity
  readonly isAuthenticated = computed(() => !!this.accessTokenSignal());
  readonly userRole = computed(() => this.currentUser()?.role ?? null);

  login(credentials: LoginRequest): Observable<TokenResponse> {
    return this.http.post<TokenResponse>(`${this.apiUrl}/login`, credentials).pipe(
      tap(response => this.handleAuthenticationSuccess(response))
    );
  }

  registerBidder(data: RegisterBidderRequest): Observable<User> {
    return this.http.post<User>(`${this.apiUrl}/register-bidder`, data);
  }

  // Updated method in auth.service.ts
registerAndSetupSupplier(
  userData: RegisterSupplierUserData, 
  profileData: RegisterSupplierProfileData
): Observable<User> {
  const payload: RegisterBidderRequest = {
    fullName: userData.fullName,
    email: userData.email,
    password: userData.password,
    companyName: profileData.companyName,
    contactPerson: profileData.contactPerson,
    taxId: profileData.taxIdNumber,                      // Maps to C# TaxId
    businessLicenseNo: profileData.businessLicenseNumber,// Maps to C# BusinessLicenseNo
    phoneNumber: profileData.phoneNumber,
    address: profileData.address
  };

  return this.http.post<User>(`${this.apiUrl}/register-bidder`, payload);
}

  // 4. Role-based Dashboard Navigation (Aligned with app.routes.ts)
  navigateToDashboard(): void {
    const role = this.userRole()?.toLowerCase();
    
    switch (role) {
      case 'admin':
        this.router.navigate(['/dashboard/admin']);
        break;

      case 'tenderofficer':
    
        this.router.navigate(['/dashboard/procurement']);
        break;

      case 'bidder':
      case 'supplier':
      default:
        this.router.navigate(['/dashboard/bidder']);
        break;
        case 'evaluator':
        this.router.navigate(['/dashboard/evaluator']);
        break;
    }
  }

  // 5. Explicit JSON Payload Refresh Token Call
  refreshToken(): Observable<TokenResponse> {
    const currentAccessToken = this.getAccessToken() || '';
    const storedRefreshToken = this.getRefreshToken() || '';

    if (!storedRefreshToken) {
      this.logout();
      return throwError(() => new Error('No refresh token available.'));
    }

    const payload: RefreshTokenRequest = {
      accessToken: currentAccessToken,
      refreshToken: storedRefreshToken
    };

    return this.http.post<TokenResponse>(`${this.apiUrl}/refresh`, payload).pipe(
      tap(response => this.handleAuthenticationSuccess(response)),
      catchError(error => {
        this.logout();
        return throwError(() => error);
      })
    );
  }

  // 6. App Initialization Bootstrap (Restores access token on page refresh)
  initializeAuthSession(): Observable<TokenResponse | null> {
    if (this.getRefreshToken()) {
      return this.refreshToken().pipe(
        catchError(() => of(null))
      );
    }
    return of(null);
  }

  logout(): void {
    const refreshToken = this.getRefreshToken();

    // Fire-and-forget token revocation request
    if (refreshToken) {
      this.http.post(`${this.apiUrl}/revoke`, { refreshToken }).subscribe({
        error: () => {} // Ignore errors on logout
      });
    }
    
    // Clear persistent storage
    localStorage.removeItem('refresh_token');
    localStorage.removeItem('user_info');
    
    // Reset in-memory signals
    this.accessTokenSignal.set(null);
    this.currentUser.set(null);
    
    this.router.navigate(['/auth/login']);
  }

  // In-Memory Access Token Getter
  getAccessToken(): string | null {
    return this.accessTokenSignal();
  }

  getRefreshToken(): string | null {
    return localStorage.getItem('refresh_token');
  }

  private handleAuthenticationSuccess(response: TokenResponse): void {
    // Keep Access Token in Memory ONLY
    this.accessTokenSignal.set(response.accessToken);
    this.currentUser.set(response.user);

    // Persist Refresh Token & Profile info in localStorage
    localStorage.setItem('refresh_token', response.refreshToken);
    localStorage.setItem('user_info', JSON.stringify(response.user));
  }

  private getStoredUser(): User | null {
    const userStr = localStorage.getItem('user_info');
    if (!userStr) return null;
    try {
      return JSON.parse(userStr) as User;
    } catch {
      return null;
    }
  }
}