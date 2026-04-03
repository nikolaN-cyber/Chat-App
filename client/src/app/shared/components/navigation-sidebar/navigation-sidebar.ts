import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { authStore } from '../../store/auth.store';
import { Router, RouterModule } from '@angular/router';
import { conversationsStore } from '../../store/conversations.store';
import { CreateConversation } from '../../../features/create-conversation/create-conversation';
import { CreateConversationData } from '../../../core/models/conversation';

@Component({
  selector: 'app-navigation-sidebar',
  imports: [
    MatButtonModule,
    MatIconModule,
    MatListModule,
    RouterModule
  ],
  template: `
  <div class="sidebar d-flex flex-column vh-100 border-end">
    <div class="p-3 d-flex flex-column justify-content-between align-items-center">
      <button [style.color]="'var(--text-color)'" [routerLink]="['/home']" title="Home" mat-icon-button>
        <mat-icon>home</mat-icon>
      </button>
    </div>

    <div class="p-3 mt-auto">
      <div class="d-flex flex-column align-items-center gap-2">
        <button (click)="createNotes()" [style.color]="'var(--text-color)'"  title="Open your notes" mat-icon-button>
          <mat-icon>edit_note</mat-icon>
        </button>
        <button [style.color]="'var(--text-color)'" (click)="goToProfilePage()" title="{{title}}" mat-icon-button>
          <mat-icon>person</mat-icon>
        </button>
      </div>
    </div>
  </div>`
})
export class NavigationSidebar {
  readonly authStore = inject(authStore);
  readonly conversationStore = inject(conversationsStore);
  private router = inject(Router);

  title = this.authStore.currentUser()?.firstName + " " + this.authStore.currentUser()?.lastName;

  goToProfilePage() {
    this.router.navigate(['/home/my-profile']);
  }

  createNotes() {
    let id = this.authStore.currentUser()?.id;
    if (id) {
      const request: CreateConversationData = {
        title: 'Notes',
        participantIds: [id]
      }
      this.conversationStore.createConversation(request);
    }
  }
}
