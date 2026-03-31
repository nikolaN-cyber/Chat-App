import { Component, computed, effect, ElementRef, inject, OnDestroy, OnInit, signal, untracked, ViewChild } from '@angular/core';
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
import { map, Subscription } from 'rxjs';
import { ChatSignalRService } from '../../core/services/chat-signalr.service';
import { environment } from '../../../environments/environment.development';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { FileService } from '../../core/services/file.service';
import { SearchConversationRequest } from '../../core/models/conversation';

@Component({
  selector: 'app-chat',
  standalone: true,
  imports: [
    MatButtonModule, MatListModule, MatProgressSpinnerModule, DatePipe,
    MatFormFieldModule, MatIconModule, MatInputModule, CommonModule,
    ReactiveFormsModule, RouterLink, MatSnackBarModule
  ],
  templateUrl: './chat.html',
  styleUrl: './chat.css',
})
export class Chat implements OnInit, OnDestroy {
  selectedConvTitle = signal('');
  @ViewChild('scrollContainer') scrollContainer!: ElementRef;

  private fb = inject(FormBuilder);
  private route = inject(ActivatedRoute);
  private snackBar = inject(MatSnackBar);
  private readonly fileService = inject(FileService);
  private signalrService = inject(ChatSignalRService);

  readonly chatStore = inject(chatStore);
  readonly convStore = inject(conversationsStore);
  readonly authStore = inject(authStore);

  public imageBaseUrl = environment.imageBaseUrl;
  private routeSub?: Subscription;

  routeId = toSignal(
    this.route.paramMap.pipe(map(params => Number(params.get('id'))))
  );

  sendMessageForm = this.fb.nonNullable.group({
    content: ['', [Validators.required, Validators.maxLength(500)]],
  });

  isGroup = computed(() => {
    const currentId = this.routeId();
    return !!this.convStore.conversations()?.find(c => c.id === currentId)?.isGroup;
  });

  messagesToDisplay = computed(() => {
    const isSearching = this.chatStore.isSearching();
    const searchResults = this.chatStore.searchResult();
    const allMessages = this.chatStore.messages();

    return isSearching ? searchResults : allMessages;
  });

  constructor() {
    effect(() => {
      const messages = this.chatStore.messages();
      const searchResults = this.chatStore.searchResult();
      if (messages.length > 0 || searchResults.length > 0) {
        untracked(() => {
          requestAnimationFrame(() => this.scrollToBottom());
        });
      }
    });
  }

  ngOnInit(): void {
    this.routeSub = this.route.paramMap.subscribe(params => {
      const id = Number(params.get('id'));
      if (id) {
        this.initializeChat(id);
      }
    });
  }

  private async initializeChat(id: number) {
    this.chatStore.clearIsSearching();
    this.chatStore.loadChat(id);
    const selectedConv = this.convStore.conversations()?.find(c => c.id === id);
    this.selectedConvTitle.set(selectedConv?.title || "Private chat");
    this.signalrService.startConnection();
    await this.signalrService.joinConversation(id);
    setTimeout(() => this.scrollToBottom(), 500);
  }

  private scrollToBottom() {
    if (this.scrollContainer) {
      const el = this.scrollContainer.nativeElement;
      el.scrollTo({
        top: el.scrollHeight,
        behavior: 'smooth'
      });
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

  onFileSelected(event: any) {
    const file: File = event.target.files[0];
    const currentConvId = this.routeId();
    if (!file || !currentConvId) return;

    this.fileService.uploadFile(file).subscribe(fileData => {
      const messagePayload = {
        content: this.sendMessageForm.value.content || '',
        conversationId: currentConvId,
        fileUrl: fileData.url,
        fileType: fileData.type
      };
      this.chatStore.sendMessage(messagePayload);
    });
  }

  openFullImage(path: string) {
    window.open(this.imageBaseUrl + path, '_blank');
  }

  getCleanUrl(fileUrl: string): string {
    if (!fileUrl) return '';
    const base = this.imageBaseUrl.endsWith('/') ? this.imageBaseUrl.slice(0, -1) : this.imageBaseUrl;
    const path = fileUrl.startsWith('/') ? fileUrl : '/' + fileUrl;
    return base + path;
  }

  searchMessages(event: Event) {
    const value = (event.target as HTMLInputElement).value;
    const currentConvId = this.routeId();
    if (!currentConvId) return;
    if (!value || value.trim().length === 0) {
      this.chatStore.clearIsSearching();
      return;
    }
    if (value.trim().length >= 2) {
      const payload: SearchConversationRequest = {
        conversationId: currentConvId,
        filter: value
      };
      this.chatStore.searchConversation(payload);
    }
  }

  deleteChat() {
    const currentConvId = this.routeId();
    if (currentConvId && confirm("Are you sure that you want to delete this chat?")) {
      this.convStore.deleteConversation(currentConvId);
    }
  }

  ngOnDestroy(): void {
    this.routeSub?.unsubscribe();
    const currentId = this.routeId();
    if (currentId) {
      this.signalrService.leaveConversation(currentId);
    }
  }
}