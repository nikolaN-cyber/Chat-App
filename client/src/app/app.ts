import { Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { authStore } from './shared/store/auth.store';
import { ChatSignalRService } from './core/services/chat-signalr.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  template: `<router-outlet></router-outlet>`
})
export class App {
  readonly authStore = inject(authStore);
  private signalrService = inject(ChatSignalRService);

  constructor() {
    this.signalrService.startConnection();
  }
}
