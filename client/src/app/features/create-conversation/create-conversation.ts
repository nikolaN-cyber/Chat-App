import { Component, inject, signal } from '@angular/core';
import { conversationsStore } from '../../shared/store/conversations.store';
import { MatFormField } from '@angular/material/input';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatProgressSpinner } from "@angular/material/progress-spinner";
import { UserSearchResponse } from '../../core/models/user';
import { MatChipsModule } from '@angular/material/chips';
import { MatInputModule } from '@angular/material/input';
import { MatLabel } from '@angular/material/input';
import { CreateConversationData } from '../../core/models/conversation';
import { Chip } from '../../shared/components/chip/chip';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-create-conversation',
  imports: [MatFormField, MatAutocompleteModule, MatProgressSpinner, MatChipsModule, MatLabel, MatInputModule, Chip, MatIconModule],
  templateUrl: './create-conversation.html'
})
export class CreateConversation {

  readonly conversationStore = inject(conversationsStore);
  selectedUsers = signal<UserSearchResponse[]>([]);
  groupTitle = signal('');

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
