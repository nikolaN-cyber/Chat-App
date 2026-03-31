import { Component, inject } from '@angular/core';
import { MatFormField } from '@angular/material/form-field';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatInputModule } from '@angular/material/input';
import { userStore } from '../../shared/store/user.store';
import { MatListModule } from '@angular/material/list';
import { MatLineModule } from '@angular/material/core';
import { chatStore } from '../../shared/store/chat.store';
import { MatProgressSpinnerModule } from "@angular/material/progress-spinner";
import { CreateConversationData, ParticipantNames } from '../../core/models/conversation';
import { conversationsStore } from '../../shared/store/conversations.store';
import { environment } from '../../../environments/environment.development';
import { MatIcon } from "@angular/material/icon";

@Component({
  selector: 'app-welcome',
  imports: [MatAutocompleteModule,
    MatFormField,
    MatAutocompleteModule,
    MatLineModule,
    MatListModule,
    MatInputModule, MatProgressSpinnerModule, MatIcon],
  templateUrl: './welcome.html',
  styleUrl: './welcome.css',
})
export class Welcome {
  readonly userStore = inject(userStore);
  readonly chatStore = inject(chatStore);
  readonly conversationStore = inject(conversationsStore);

  public imageBaseUrl = environment.imageBaseUrl;

  constructor() {
    this.userStore.searchByUsername(this.userStore.searchFilter);
  }

  onSearchChange(event: Event) {
    const value = (event.target as HTMLInputElement).value;
    this.userStore.updateFilter(value);
  }

  openPrivateChat(user: ParticipantNames) {
    const request: CreateConversationData = {
      title: '',
      participantIds: [user.userId]
    }
    this.conversationStore.createConversation(request);
  }

  getStatusIcon(emoji: string): string {
  const iconMap: Record<string, string> = {
    'house': 'home',
    'palm-tree': 'beach_access',
    'onvacation': 'beach_access',
    'workingremotely': 'home'
  };
  return iconMap[emoji] || 'sentiment_satisfied';
}
}
