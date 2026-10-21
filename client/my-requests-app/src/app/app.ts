import { HttpErrorResponse } from '@angular/common/http';
import { JsonPipe } from '@angular/common';
import { Component, inject, isDevMode, signal } from '@angular/core';
import { DevLogin } from './dev/dev-login';
import { MyRequestsResponse } from './my-requests/my-requests.model';
import { MyRequestsService } from './my-requests/my-requests.service';

/**
 * שלד זמני לבדיקת החיבור ל-API (שלב 9).
 * בשלב 10 הוא יוחלף ברכיב "הבקשות שלי".
 */
@Component({
  selector: 'app-root',
  imports: [DevLogin, JsonPipe],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  private readonly service = inject(MyRequestsService);

  protected readonly isDev = isDevMode();
  protected readonly data = signal<MyRequestsResponse | null>(null);
  protected readonly error = signal<string | null>(null);

  constructor() {
    this.reload();
  }

  protected reload(): void {
    this.error.set(null);
    this.service.getMyRequests().subscribe({
      next: (response) => this.data.set(response),
      error: (e: HttpErrorResponse) => {
        this.data.set(null);
        this.error.set(`הקריאה נכשלה, קוד ${e.status}`);
      },
    });
  }
}
