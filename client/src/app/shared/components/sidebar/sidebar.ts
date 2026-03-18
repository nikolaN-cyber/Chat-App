import { Component, effect, inject, OnInit } from '@angular/core';
import { conversationsStore } from '../../store/conversations.store';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatListModule } from '@angular/material/list';
import { themeStore } from '../../store/theme.store';
import { authStore } from '../../store/auth.store';
import { RouterLink, RouterLinkActive } from "@angular/router";

@Component({
  selector: 'app-sidebar',
  imports: [
    MatButtonModule,
    MatIconModule,
    MatListModule,
    RouterLink,
    RouterLinkActive
],
  templateUrl: './sidebar.html',
  styleUrl: './sidebar.css',
})
export class Sidebar {

  readonly conversationsStore = inject(conversationsStore);
  readonly themeStore = inject(themeStore);
  readonly authStore = inject(authStore);

  constructor() {
    effect(() => {
      const user = this.authStore.currentUser();
      if( user && user.accessToken){
        this.conversationsStore.getUserConversations();
      }
    })
  }
}
