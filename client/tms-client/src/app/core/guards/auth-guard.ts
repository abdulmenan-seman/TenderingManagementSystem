import { inject } from '@angular/core';
import { CanActivateFn, Router, ActivatedRouteSnapshot } from '@angular/router';
import { AuthService } from '../services/auth';
import { map } from 'rxjs';

export const unauthGuard: CanActivateFn = () => {
  const authService = inject(AuthService);

  if (authService.isAuthenticated()) {
    authService.navigateToDashboard();
    return false;
  }

  return true;
};
export const roleGuard: CanActivateFn = (route: ActivatedRouteSnapshot) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  // Extract allowed roles defined in route data (e.g., data: { roles: ['Admin'] })
  const expectedRoles = (route.data['roles'] as Array<string>) || [];

  if (authService.currentUser()) {
    return checkUserRole(authService, router, expectedRoles);
  }

  // Attempt session recovery via cookie on page refresh
  return authService.checkSession().pipe(
    map(() => checkUserRole(authService, router, expectedRoles))
  );
};

function checkUserRole(authService: AuthService, router: Router, expectedRoles: string[]): boolean {
  // Case 1: Not logged in at all -> redirect to login
  if (!authService.isAuthenticated()) {
    router.navigate(['/auth/login']);
    return false;
  }

  const userRoles = authService.currentUser()?.roles || [];

  // Case 2: User has at least one of the required roles -> allow access
  const hasRequiredRole = expectedRoles.some(role => userRoles.includes(role));
  if (hasRequiredRole) {
    return true;
  }

  // Case 3: Logged in but wrong role -> send to their own role-based dashboard
  authService.navigateToDashboard();
  return false;
}