// Authentication & User DTOs
export interface LoginRequestDto {
  email: string;
  password: string;
}

export interface AuthResponseDto {
  userId: number;
  fullName: string;
  email: string;
  token: string;
  roles: string[];
}

export interface RegisterUserRequestDto {
  fullName: string;
  email: string;
  password: string;
}

export interface UserResponseDto {
  id: number;
  fullName: string;
  email: string;
  isActive: boolean;
  roles: string[];
}

// Supplier Profile DTOs
export interface CreateSupplierProfileRequestDto {
  userId: number;
  companyName: string;
  taxIdNumber: string;
  businessLicenseNumber: string;
  address: string;
  phoneNumber: string;
}

export interface SupplierProfileResponseDto {
  id: number;
  userId: number;
  companyName: string;
  taxIdNumber: string;
  businessLicenseNumber: string;
  address: string;
  phoneNumber: string;
}