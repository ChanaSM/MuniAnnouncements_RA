import { Component, isDevMode, viewChild } from '@angular/core';
import { DevLogin } from './dev/dev-login';
import { MyRequests } from './my-requests/my-requests';

/** מדמה את הדף באזור האישי שמארח את הרכיב "הבקשות שלי". */
@Component({
  selector: 'app-root',
  imports: [DevLogin, MyRequests],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  protected readonly isDev = isDevMode();
  private readonly list = viewChild(MyRequests);

  /** אחרי החלפת משתמש בכלי הפיתוח, טוענים את הרשימה מחדש. */
  protected reload(): void {
    this.list()?.load();
  }
}
