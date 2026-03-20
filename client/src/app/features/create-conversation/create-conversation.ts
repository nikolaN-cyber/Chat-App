import { Component, inject, signal } from '@angular/core';
import { userStore } from '../../shared/store/user.store';
import { conversationsStore } from '../../shared/store/conversations.store';
import { MatFormField } from '@angular/material/input';
import { MatAutocompleteModule, MatAutocompleteSelectedEvent } from '@angular/material/autocomplete';
import { MatProgressSpinner } from "@angular/material/progress-spinner";
import { UserSearchResponse } from '../../core/models/user';
import { MatChipsModule } from '@angular/material/chips';
import { MatInputModule } from '@angular/material/input';
import { MatLabel } from '@angular/material/input';
import { MatIcon } from '@angular/material/icon';
import { CreateConversationData } from '../../core/models/conversation';

@Component({
  selector: 'app-create-conversation',
  imports: [MatFormField, MatAutocompleteModule, MatProgressSpinner, MatChipsModule, MatLabel, MatIcon, MatInputModule],
  templateUrl: './create-conversation.html',
  styleUrl: './create-conversation.css',
})
export class CreateConversation {
  readonly userStore = inject(userStore);
  readonly conversationStore = inject(conversationsStore);

  groupTitle = signal('');

  selectedUsers = signal<UserSearchResponse[]>([]);

  constructor() {
    this.userStore.searchByUsername(this.userStore.searchFilter);
  }

  onSearchChange(event: Event) {
    const value = (event.target as HTMLInputElement).value;
    this.userStore.updateFilter(value);
  }

  selected(event: MatAutocompleteSelectedEvent): void {
    const user = event.option.value as UserSearchResponse;
    if (!this.selectedUsers().some(u => u.id === user.id)) {
      this.selectedUsers.update(users => [...users, user]);
    }
    this.userStore.updateFilter('');
  }
  remove(user: UserSearchResponse): void {
    this.selectedUsers.update(users => users.filter(u => u.id !== user.id));
  }

  createChat() {
    const ids = this.selectedUsers().map(u => u.id);
    const request : CreateConversationData = {
      title: this.groupTitle(),
      participantIds: ids
    };
    this.conversationStore.createConversation(request);
    this.selectedUsers.set([]);
    this.groupTitle.set('');
  }
}
