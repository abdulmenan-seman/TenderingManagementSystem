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
  private apiUrl = `${environment.apiUrl}/api/v1/users`;

  getUsers(query?: UserFilterQuery): Observable<PagedResult<UserListItem>> {
    let params = new HttpParams();
    if (query) {
      if (query.searchTerm) params = params.set('searchTerm', query.searchTerm);
      if (query.role) params = params.set('role', query.role);
      if (query.isActive !== undefined) params = params.set('isActive', query.isActive);
      params = params.set('pageNumber', query.pageNumber);
      params = params.set('pageSize', query.pageSize);
    }

    return this.http.get<PagedResult<UserListItem>>(this.apiUrl, { params });
  }

  getUserById(id: number): Observable<UserListItem> {
    return this.http.get<UserListItem>(`${this.apiUrl}/${id}`);
  }

  createUser(dto: CreateUserDto): Observable<UserListItem> {
    return this.http.post<UserListItem>(this.apiUrl, dto);
  }

  updateUser(id: number, dto: UpdateUserDto): Observable<UserListItem> {
    return this.http.put<UserListItem>(`${this.apiUrl}/${id}`, dto);
  }

  toggleUserStatus(id: number, isActive: boolean): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/status`, { isActive });
  }

  resetPassword(id: number, dto: ResetPasswordDto): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/reset-password`, dto);
  }

  softDeleteUser(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  getLoginActivityLogs(userId?: number): Observable<LoginActivityLog[]> {
    let params = new HttpParams();
    if (userId) params = params.set('userId', userId.toString());
    return this.http.get<LoginActivityLog[]>(`${this.apiUrl}/logs`, { params });
  }
}