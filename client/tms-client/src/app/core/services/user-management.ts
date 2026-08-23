import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  UserListItem,
  UserFilterQuery,
  PagedResult,
  CreateUserDto,
  UpdateUserDto,
  ResetPasswordDto,
  LoginActivityLog
} from '../models/user-management.model';

@Injectable({
  providedIn: 'root'
})
export class UserManagementService {
  private http = inject(HttpClient);

  // Type assertion resolves potential environment typing issues
  private baseUrl = (environment as { apiUrl: string }).apiUrl;
  private usersUrl = `${this.baseUrl}/users`;
  private authUrl = `${this.baseUrl}/auth`;

  getUsers(query?: UserFilterQuery): Observable<PagedResult<UserListItem>> {
    let params = new HttpParams();
    if (query) {
      if (query.searchTerm) params = params.set('searchTerm', query.searchTerm);
      if (query.role) params = params.set('role', query.role);
      if (query.isActive !== undefined) params = params.set('isActive', query.isActive);
      params = params.set('pageNumber', query.pageNumber);
      params = params.set('pageSize', query.pageSize);
    }

    // Pointing directly to GET /api/v1/users to fix 404
    return this.http.get<PagedResult<UserListItem>>(`${this.usersUrl}`, { params });
  }

  getUserById(id: number): Observable<UserListItem> {
    return this.http.get<UserListItem>(`${this.usersUrl}/${id}`);
  }

  createUser(dto: CreateUserDto): Observable<UserListItem> {
    return this.http.post<UserListItem>(`${this.authUrl}/create-staff`, dto);
  }

  updateUser(id: number, dto: UpdateUserDto): Observable<UserListItem> {
    return this.http.put<UserListItem>(`${this.authUrl}/staff/${id}`, dto);
  }

  toggleUserStatus(id: number, isActive: boolean): Observable<void> {
    return this.http.patch<void>(`${this.authUrl}/staff/${id}/status`, isActive);
  }

  resetPassword(id: number, dto: ResetPasswordDto): Observable<void> {
    return this.http.post<void>(`${this.authUrl}/staff/${id}/reset-password`, dto);
  }

  softDeleteUser(id: number): Observable<void> {
    return this.http.delete<void>(`${this.usersUrl}/${id}`);
  }

  getLoginActivityLogs(userId?: number): Observable<LoginActivityLog[]> {
    let params = new HttpParams();
    if (userId) params = params.set('userId', userId.toString());
    return this.http.get<LoginActivityLog[]>(`${this.usersUrl}/logs`, { params });
  }
}