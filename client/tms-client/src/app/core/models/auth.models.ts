export interface User {
  id: number;
  email: string;
  fullName: string;
  role: 'Admin' | 'TenderOfficer' | 'Evaluator' | 'Bidder';
  companyName?: string;
}

export interface TokenResponse {
  accessToken: string;
  refreshToken: string;
  refreshTokenExpiration: string;
  user: User;
}

export interface RegisterBidderRequest {
  email: string;
  password: string;
  fullName: string;
  companyName: string;
  contactPerson: string;
  businessLicenseNo: string; // Matched with C# BusinessLicenseNo
  taxId: string;             // Matched with C# TaxId
  address: string;
  phoneNumber: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RefreshTokenRequest {
  accessToken: string;
  refreshToken: string;
}