import { Component, ElementRef, computed, inject, signal, viewChild } from '@angular/core';
import { NormalizedRequest } from './my-requests.model';
import { MyRequestsService } from './my-requests.service';
import { loadReadKeys, requestKey, saveReadKeys } from './read-marks';

type ListState =
  | { kind: 'loading' }
  | { kind: 'error' }
  | { kind: 'ready'; requests: NormalizedRequest[] };

/** הרכיב "הבקשות שלי": טעינה, שגיאה עם ניסיון חוזר, רשימה ריקה, ורשימת בקשות. */
@Component({
  selector: 'app-my-requests',
  templateUrl: './my-requests.html',
  styleUrl: './my-requests.scss',
})
export class MyRequests {
  private readonly service = inject(MyRequestsService);
  private readonly dialog = viewChild<ElementRef<HTMLDialogElement>>('actionDialog');
  private readonly readKeys = signal(loadReadKeys());

  protected readonly state = signal<ListState>({ kind: 'loading' });
  protected readonly selected = signal<NormalizedRequest | null>(null);
  protected readonly requests = computed(() => {
    const state = this.state();
    return state.kind === 'ready' ? state.requests : [];
  });

  constructor() {
    this.load();
  }

  /** נקראת גם מכפתור "נסה שוב" וגם מבחוץ אחרי החלפת משתמש בפיתוח. */
  load(): void {
    this.state.set({ kind: 'loading' });
    this.service.getMyRequests().subscribe({
      next: (response) => this.state.set({ kind: 'ready', requests: response.requests }),
      error: () => this.state.set({ kind: 'error' }),
    });
  }

  protected isRead(request: NormalizedRequest): boolean {
    return this.readKeys().has(requestKey(request));
  }

  protected openAction(request: NormalizedRequest): void {
    this.selected.set(request);
    this.dialog()?.nativeElement.showModal();
  }

  /** אישור: מסמן את הבקשה כנקראה וסוגר את החלונית. */
  protected confirm(): void {
    const request = this.selected();
    if (request) {
      const next = new Set(this.readKeys());
      next.add(requestKey(request));
      this.readKeys.set(next);
      saveReadKeys(next);
    }
    this.closeDialog();
  }

  protected closeDialog(): void {
    this.dialog()?.nativeElement.close();
  }
}
