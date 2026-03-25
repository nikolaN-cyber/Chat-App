import { Component, computed, effect, ElementRef, inject, OnDestroy, signal, untracked, ViewChild } from '@angular/core';
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
import { ActivatedRoute, RouterLink } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { map } from 'rxjs';
import { ChatSignalRService } from '../../core/services/chat-signalr.service';
import { environment } from '../../../environments/environment.development';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'app-chat',
  imports: [
    MatButtonModule,
    MatListModule,
    MatProgressSpinnerModule,
    DatePipe,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    MatSnackBarModule
],
  templateUrl: './chat.html',
  styleUrl: './chat.css',
})
export class Chat implements OnDestroy {

  selectedConvTitle = signal('');
  @ViewChild('scrollContainer') scrollContainer!: ElementRef;

  private fb = inject(FormBuilder);
  private route = inject(ActivatedRoute);

  public imageBaseUrl = environment.imageBaseUrl; 

  routeId = toSignal(
    this.route.paramMap.pipe(
      map(params => Number(params.get('id')))
    )
  );

  sendMessageForm = this.fb.nonNullable.group({
    content: ['', [Validators.required, Validators.maxLength(500)]],
  });

  private snackBar = inject(MatSnackBar);
  readonly chatStore = inject(chatStore);
  readonly convStore = inject(conversationsStore);
  readonly authStore = inject(authStore);
  private signalrService = inject(ChatSignalRService);

  isGroup = computed(() => {
    const currentId = this.routeId();
    const conversations = this.convStore.conversations;
    return !!conversations()?.find(c => c.id === currentId)?.isGroup;
  })

  constructor() {

    effect(() => {
      const lastAdded = this.chatStore.lastAdded();
      const lastRemoved = this.chatStore.lastRemoved();

      if (lastAdded) {
        this.showToast(`User ${lastAdded} was added`, 'success-snackbar');
      }

      if (lastRemoved) {
        this.showToast(`User ${lastRemoved} was removed`, 'warning-snackbar');
      }
    });

    effect(() => {

      const currentId = this.routeId();
      if (currentId === undefined || currentId === null) return;

      const selectedConv = this.convStore.conversations()?.find(c => c.id === currentId);
      this.selectedConvTitle.set(selectedConv?.title || "Private chat");

      untracked(() => {
        if (this.chatStore.currentConversationId() !== currentId) {
          this.chatStore.loadChat(currentId);
        } 
        this.signalrService.joinConversation(currentId);
      });
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
    if (currentConvId && confirm("Are you sure that you want to delete this chat?")) {
      this.convStore.deleteConversation(currentConvId);
    }
  }

  showToast(message: string, panelClass: string){
    this.snackBar.open(message, 'Close', {
      duration: 3000,
      horizontalPosition: 'right',
      verticalPosition: 'bottom',
      panelClass: [panelClass]
    });
  }

  ngOnDestroy(): void {
    const currentId = this.routeId();
    if (currentId) {
      this.signalrService.leaveConversation(currentId);
    }
  }
}