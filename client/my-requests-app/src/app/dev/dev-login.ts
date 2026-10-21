import { HttpClient, HttpParams } from '@angular/common/http';
import { Component, inject, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { clearToken, setToken } from '../auth/token-storage';

/**
 * *** כלי פיתוח בלבד ***
 * מדמה את האזור האישי: מבקש מהשרת טוקן עבור ת"ז שמקלידים, ושומר אותו.
 * מוצג רק כש-isDevMode() אמיתי (ראו app.html). למחיקה: התיקייה הזו והשורה ב-app.html.
 */
@Component({
  selector: 'app-dev-login',
  imports: [FormsModule],
  template: `
    <form class="dev-login" (ngSubmit)="login()">
      <label>
        ת"ז לבדיקה (פיתוח בלבד)
        <input name="idNumber" [(ngModel)]="idNumber" inputmode="numeric" autocomplete="off" />
      </label>
      <button type="submit" [disabled]="busy()">התחברות</button>
      <button type="button" (click)="logout()">ניתוק</button>
      @if (error()) {
        <span role="alert">{{ error() }}</span>
      }
    </form>
  `,
  styles: `
    .dev-login {
      display: flex;
      flex-wrap: wrap;
      gap: 0.5rem;
      align-items: end;
      padding: 0.75rem;
      margin-block-end: 1rem;
      border: 1px dashed #888;
      border-radius: 6px;
    }
  `,
})
export class DevLogin {
  private readonly http = inject(HttpClient);

  protected idNumber = '012345678';
  protected readonly busy = signal(false);
  protected readonly error = signal<string | null>(null);

  /** נשלח אחרי כניסה או ניתוק, כדי שהעמוד יטען מחדש את הבקשות. */
  readonly changed = output<void>();

  protected login(): void {
    this.busy.set(true);
    this.error.set(null);

    const params = new HttpParams().set('idNumber', this.idNumber);
    this.http.post<{ token: string }>('/api/dev/token', null, { params }).subscribe({
      next: ({ token }) => {
        setToken(token);
        this.busy.set(false);
        this.changed.emit();
      },
      error: () => {
        this.busy.set(false);
        this.error.set('לא הצלחנו להנפיק טוקן. האם השרת רץ?');
      },
    });
  }

  protected logout(): void {
    clearToken();
    this.changed.emit();
  }
}
