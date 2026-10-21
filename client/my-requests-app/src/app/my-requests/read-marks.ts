import { NormalizedRequest } from './my-requests.model';

/**
 * סימון "נקרא" נשמר בדפדפן בלבד (אין לו מקום בשרת, וזה מחוץ לדרישות).
 * המפתח כולל את תאריך העדכון האחרון: כשהבקשה מתעדכנת, הסימון מתאפס ודורש אישור חדש.
 */
const STORAGE_KEY = 'read-actions';

export function requestKey(request: NormalizedRequest): string {
  return `${request.requestId}|${request.lastUpdate}`;
}

export function loadReadKeys(): Set<string> {
  try {
    const parsed: unknown = JSON.parse(localStorage.getItem(STORAGE_KEY) ?? '[]');
    return new Set(Array.isArray(parsed) ? parsed.filter((k) => typeof k === 'string') : []);
  } catch {
    return new Set();
  }
}

export function saveReadKeys(keys: Set<string>): void {
  try {
    localStorage.setItem(STORAGE_KEY, JSON.stringify([...keys]));
  } catch {
    // אין אחסון זמין: הסימון יישאר רק עד רענון העמוד
  }
}
