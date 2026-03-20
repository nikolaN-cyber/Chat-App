import { Component, inject, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { authStore } from './shared/store/auth.store';
import { ChatSignalRService } from './core/services/chat-signalr.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  readonly authStore = inject(authStore);
  private signalrService = inject(ChatSignalRService);

  constructor() {
    this.signalrService.startConnection();
  }
}
