import { DestroyRef, Injectable, NgZone, inject } from '@angular/core';
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';
import { Subject } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class RealtimeService {
  private readonly zone = inject(NgZone);
  private readonly changed = new Subject<void>();
  readonly changes$ = this.changed.asObservable();
  private readonly connection = new HubConnectionBuilder()
    .withUrl('/hubs/restaurant')
    .withAutomaticReconnect({ nextRetryDelayInMilliseconds: () => 5000 })
    .configureLogging(LogLevel.Warning)
    .build();
  private retryTimer?: ReturnType<typeof setTimeout>;
  private disposed = false;

  constructor() {
    const refresh = () => this.zone.run(() => this.changed.next());
    this.connection.on('WorkspaceChanged', refresh);
    this.connection.onreconnected(refresh);
    inject(DestroyRef).onDestroy(() => {
      this.disposed = true;
      clearTimeout(this.retryTimer);
      void this.connection.stop();
      this.changed.complete();
    });
    // Automatic reconnect does not retry the initial start.
    void this.start();
  }

  private async start(): Promise<void> {
    try {
      await this.connection.start();
      if (!this.disposed) this.zone.run(() => this.changed.next());
    } catch {
      if (!this.disposed) this.retryTimer = setTimeout(() => void this.start(), 5000);
    }
  }
}
