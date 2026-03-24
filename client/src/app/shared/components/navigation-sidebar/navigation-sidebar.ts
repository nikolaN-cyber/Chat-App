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
  templateUrl: './navigation-sidebar.html',
  styleUrl: './navigation-sidebar.css',
})
export class NavigationSidebar {
  readonly authStore = inject(authStore);
  private router = inject(Router);

  title = this.authStore.currentUser()?.firstName + " " + this.authStore.currentUser()?.lastName;

  goToProfilePage(){
    this.router.navigate(['/home/my-profile']);
  }
}
