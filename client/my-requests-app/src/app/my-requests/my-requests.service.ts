import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { MyRequestsResponse } from './my-requests.model';

/** השכבה היחידה שמדברת עם ה-API. הרכיב לא יודע מה זה HTTP. */
@Injectable({ providedIn: 'root' })
export class MyRequestsService {
  private readonly http = inject(HttpClient);

  /** הת"ז לא נשלחת: השרת מזהה את התושב לפי הטוקן שה-interceptor מצרף. */
  getMyRequests(): Observable<MyRequestsResponse> {
    return this.http.get<MyRequestsResponse>('/api/my-requests');
  }
}
