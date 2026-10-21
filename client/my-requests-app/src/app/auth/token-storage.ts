/**
 * מקום אחד שבו הטוקן נשמר ונקרא.
 * באזור האישי האמיתי הטוקן כבר קיים אצלם; כאן אנחנו מדמים את זה באמצעות localStorage.
 * עוטפים ב-try/catch כי localStorage עלול להיות חסום (מצב גלישה פרטית, מדיניות דפדפן).
 */
const TOKEN_KEY = 'token';

export function getToken(): string | null {
  try {
    return localStorage.getItem(TOKEN_KEY);
  } catch {
    return null;
  }
}

export function setToken(token: string): void {
  try {
    localStorage.setItem(TOKEN_KEY, token);
  } catch {
    // אין אחסון זמין: הקריאות הבאות ילכו בלי טוקן ויקבלו 401
  }
}

export function clearToken(): void {
  try {
    localStorage.removeItem(TOKEN_KEY);
  } catch {
    // אין מה לנקות
  }
}
