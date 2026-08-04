export interface AuthUser {
  userId: string;
  email: string;
  name: string;
  role: 'Admin' | 'Staff';
}

export interface LoginResult extends AuthUser {
  accessToken: string;
}
