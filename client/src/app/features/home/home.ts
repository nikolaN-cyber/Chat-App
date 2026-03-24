import { Component } from '@angular/core';
import { Sidebar } from '../../shared/components/sidebar/sidebar';
import { RouterOutlet } from '@angular/router';
import { NavigationSidebar } from '../../shared/components/navigation-sidebar/navigation-sidebar';

@Component({
  selector: 'app-home',
  imports: [Sidebar, NavigationSidebar, RouterOutlet],
  template: `
  <div class="d-flex h-100 overflow-hidden main-container">
    <app-navigation-sidebar></app-navigation-sidebar>
    <app-sidebar class="flex-shrink-0 border-end"></app-sidebar>
    <main class="flex-grow-1 h-100 position-relative bg-white">
        <router-outlet></router-outlet>
    </main>
  </div>`
})
export class Home {
}
