/** בקשה אחת כפי שה-API מחזיר אותה. אין כאן ת"ז: השרת לא שולח אותה. */
export interface NormalizedRequest {
  requestId: string;
  serviceName: string;
  authorityName: string;
  status: string;
  actionRequired: boolean;
  isOverdue: boolean;
  /** dd/MM/yyyy */
  submittedAt: string;
  /** dd/MM/yyyy */
  lastUpdate: string;
}

export interface MyRequestsResponse {
  requests: NormalizedRequest[];
}
