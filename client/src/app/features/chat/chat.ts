import { Component, effect, inject, input, untracked } from '@angular/core';
import { conversationsStore } from '../../shared/store/conversations.store';
import { chatStore } from '../../shared/store/chat.store';
import { authStore } from '../../shared/store/auth.store';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatListModule } from '@angular/material/list';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { CommonModule, DatePipe } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { map } from 'rxjs';

@Component({
  selector: 'app-chat',
  imports: [
    MatIconModule,
    MatButtonModule,
    MatListModule,
    MatProgressSpinnerModule,
    DatePipe,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    CommonModule,
    ReactiveFormsModule,
  ],
  templateUrl: './chat.html',
  styleUrl: './chat.css',
})
export class Chat {

  private fb = inject(FormBuilder);
  private route = inject(ActivatedRoute);

  routeId = toSignal(
  this.route.paramMap.pipe(
    map(params => Number(params.get('id')))
  )
);

  sendMessageForm = this.fb.nonNullable.group({
    content: ['', [Validators.required, Validators.maxLength(500)]],
  });

  readonly chatStore = inject(chatStore);
  readonly convStore = inject(conversationsStore);
  readonly authStore = inject(authStore);

  constructor() {
    effect(() => {
      const currentId = this.routeId();
      if (currentId === undefined || currentId === null) return;

      const selectedConv = this.convStore.conversations()?.find(c => c.id === currentId);
      const myId = this.authStore.currentUser()?.id;
      const targetId = selectedConv?.participantIds.find(pid => pid !== myId);

      if (targetId) {
        untracked(() => { this.chatStore.loadChat(targetId) });
      }
    })
  }

  onSubmit() {
    const currentConvId = this.routeId();

    if (this.sendMessageForm.valid && currentConvId) {
      const messagePayload = {
        content: this.sendMessageForm.controls.content.value,
        conversationId: currentConvId
      };

      this.chatStore.sendMessage(messagePayload);
      this.sendMessageForm.reset();
    }
  }
}