import { Component, inject, model, signal } from '@angular/core';
import { MatChipsModule } from "@angular/material/chips";
import { MatFormField, MatLabel } from '@angular/material/form-field';
import { MatAutocompleteModule, MatAutocompleteSelectedEvent } from '@angular/material/autocomplete';
import { MatProgressSpinner } from '@angular/material/progress-spinner';
import { MatIcon } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { conversationsStore } from '../../store/conversations.store';
import { userStore } from '../../store/user.store';
import { UserSearchResponse } from '../../../core/models/user';

@Component({
  selector: 'app-chip',
  imports: [MatFormField, MatAutocompleteModule, MatProgressSpinner, MatChipsModule, MatLabel, MatIcon, MatInputModule],
  templateUrl: './chip.html',
  styleUrl: './chip.css',
  standalone: true
})
export class Chip {
  readonly userStore = inject(userStore);
    readonly conversationStore = inject(conversationsStore);
  
    groupTitle = signal('');
  
    selectedUsers = model<UserSearchResponse[]>([]);
  
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
}
