import { Component, effect, ElementRef, inject, input, signal, untracked, ViewChild } from '@angular/core';
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
import { ChatSignalRService } from '../../core/services/chat-signalr.service';

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

  selectedConvTitle = signal('');
  @ViewChild('scrollContainer') scrollContainer!: ElementRef;

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
  private signalrService = inject(ChatSignalRService);

  constructor() {

    this.signalrService.startConnection();

    effect(() => {
      const currentId = this.routeId();
      if (currentId === undefined || currentId === null) return;

      setTimeout(() => {
        this.signalrService.joinConversation(currentId);
      }, 500);

      console.log(currentId);
      const selectedConv = this.convStore.conversations()?.find(c => c.id === currentId);
      this.selectedConvTitle.set(selectedConv?.title || "Private chat");

      untracked(() => { this.chatStore.loadChat(currentId)});
      
    });

    effect(() => {
      const messages = this.chatStore.messages();
      if (messages.length > 0) {
        untracked(() => {
          setTimeout(() => this.scrollToBottom(), 100);
        });
      }
    });
  }

  private scrollToBottom() {
    if (this.scrollContainer) {
      const el = this.scrollContainer.nativeElement;
      el.scrollTop = el.scrollHeight;
    }
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

  deleteChat() {
    const currentConvId = this.routeId();
    if (currentConvId && confirm("Are you sure that you want to delete this chat?")){
      this.convStore.deleteConversation(currentConvId);
    }
  }
}