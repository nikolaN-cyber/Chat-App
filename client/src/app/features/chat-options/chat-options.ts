import { Component, computed, effect, inject, signal, untracked } from '@angular/core';
import { chatStore } from '../../shared/store/chat.store';
import { authStore } from '../../shared/store/auth.store';
import { toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { map } from 'rxjs';
import { MatIconModule } from '@angular/material/icon';
import { Chip } from '../../shared/components/chip/chip';
import { MatButtonModule } from '@angular/material/button';
import { CreateConversationData, ParticipantNames } from '../../core/models/conversation';
import { conversationsStore } from '../../shared/store/conversations.store';
import { UserSearchResponse } from '../../core/models/user';

@Component({
  selector: 'app-chat-options',
  imports: [RouterModule, MatIconModule, MatButtonModule, Chip],
  templateUrl: './chat-options.html',
  styleUrl: './chat-options.css',
})
export class ChatOptions {

  readonly chatStore = inject(chatStore);
  readonly conversationStore = inject(conversationsStore)
  readonly authStore = inject(authStore);
  private route = inject(ActivatedRoute);

  routeId = toSignal(
    this.route.paramMap.pipe(
      map(params => Number(params.get('id')))
    )
  );

  selectedUsers = signal<UserSearchResponse[]>([]);
  readonly isGroup = computed(() => {
    const conversations = this.conversationStore.conversations;
    const currentId = this.routeId();
    return !!conversations()?.find(c => c.id === currentId)?.isGroup;
  })

  constructor() {
    const currentId = this.routeId();
    if (currentId && currentId !== this.chatStore.currentConversationId()) {
      this.chatStore.loadChat(currentId);
    }
    effect(() => {
      const users = this.selectedUsers();
      if (users.length > 0) {
        const ids = users.map(u => u.id);
        this.chatStore.addUsers(ids);

        untracked(() => {
          this.selectedUsers.set([]);
        });
      }
    });
  }

  openPrivateChat(user: ParticipantNames) {
    const request: CreateConversationData = {
      title: '',
      participantIds: [user.userId]
    }
    this.conversationStore.createConversation(request);
  }

  removeUser(userId: number) {
    if (userId && confirm("Are you sure you want to remove this user?")) {
      this.chatStore.removeUser(userId);
    }
  }
}
