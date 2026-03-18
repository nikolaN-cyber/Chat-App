import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { authStore } from '../../shared/store/auth.store';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const AuthStore = inject(authStore);
 
  const token = AuthStore.currentUser()?.accessToken;

  const excludedUrls = ['/api/Auth/login', '/api/Auth/register'];
  const isExcluded = excludedUrls.some(url => req.url.includes(url));

  if (token && !isExcluded) {
    const authReq = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
    return next(authReq);
  }

  return next(req);
};