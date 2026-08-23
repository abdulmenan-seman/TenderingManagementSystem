export interface UserListItem {
  id: number;
  fullName: string;
  email: string;
  roles: string[];
  isActive: boolean;
  isDeleted: boolean;
  createdAt?: string;
  lastLoginAt?: string;
  failedLoginAttempts?: number;
}

export interface UserFilterQuery {
  searchTerm?: string;
  role?: string;
  isActive?: boolean;
  pageNumber: number;
  pageSize: number;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
}

export interface CreateUserDto {
  fullName: string;
  email: string;
  role: string; // Changed from string[] to string to match AuthController
  password?: string;
}

export interface UpdateUserDto {
  fullName: string;
  email: string;
  role: string; // Changed from string[] to string to match AuthController
  isActive: boolean;
}

export interface ResetPasswordDto {
  newPassword: string;
  mustChangePasswordOnLogin?: boolean;
}

export interface LoginActivityLog {
  id: string;
  userId: number;
  userEmail: string;
  ipAddress: string;
  userAgent: string;
  isSuccess: boolean;
  failureReason?: string;
  timestamp: string;
}