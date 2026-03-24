import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { authStore } from '../../store/auth.store';
import { Router, RouterModule } from '@angular/router';

@Component({
  selector: 'app-navigation-sidebar',
  imports: [
    MatButtonModule,
    MatIconModule,
    MatListModule,
    RouterModule
  ],
  template: `
  <div class="sidebar d-flex flex-column vh-100 border-end">
    <div class="p-3 d-flex flex-column justify-content-between align-items-center">
      <button [style.color]="'var(--text-color)'" [routerLink]="['/home']" title="Home" mat-icon-button>
        <mat-icon>home</mat-icon>
      </button>
    </div>

    <div class="p-3 mt-auto">
      <div class="d-flex flex-column align-items-center gap-2">
        <button [style.color]="'var(--text-color)'" (click)="goToProfilePage()" title="{{title}}" mat-icon-button>
        <mat-icon>person</mat-icon>
        </button>
        <span></span>
      </div>
    </div>
  </div>`
})
export class NavigationSidebar {
  readonly authStore = inject(authStore);
  private router = inject(Router);

  title = this.authStore.currentUser()?.firstName + " " + this.authStore.currentUser()?.lastName;

  goToProfilePage() {
    this.router.navigate(['/home/my-profile']);
  }
}
