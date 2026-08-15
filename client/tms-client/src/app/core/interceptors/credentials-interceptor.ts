import { HttpInterceptorFn } from '@angular/common/http';

export const credentialsInterceptor: HttpInterceptorFn = (req, next) => {
  const clonedReq = req.clone({
    withCredentials: true // Automatically attaches HttpOnly cookies & XSRF headers
  });
  return next(clonedReq);
};