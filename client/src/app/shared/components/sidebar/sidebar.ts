import { Component, inject, OnInit } from '@angular/core';
import { conversationsStore } from '../../store/conversations.store';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatListModule } from '@angular/material/list';
import { themeStore } from '../../store/theme.store';

@Component({
  selector: 'app-sidebar',
  imports: [
    MatButtonModule,
    MatIconModule,
    MatListModule
  ],
  templateUrl: './sidebar.html',
  styleUrl: './sidebar.css',
})
export class Sidebar implements OnInit {

  readonly conversationsStore = inject(conversationsStore);
  readonly themeStore = inject(themeStore);

  ngOnInit(){
    this.conversationsStore.getUserConversations();
  }
}
