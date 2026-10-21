import { HttpInterceptorFn } from '@angular/common/http';
import { getToken } from './token-storage';

/**
 * מצרף Authorization: Bearer לכל קריאה ל-API שלנו.
 * רק לכתובות שמתחילות ב-/api/, כדי שהטוקן לעולם לא יישלח לדומיין אחר.
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const token = getToken();

  if (!token || !req.url.startsWith('/api/')) {
    return next(req);
  }

  return next(req.clone({ setHeaders: { Authorization: `Bearer ${token}` } }));
};
