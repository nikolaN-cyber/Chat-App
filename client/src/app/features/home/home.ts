import { Component } from '@angular/core';
import { Sidebar } from '../../shared/components/sidebar/sidebar';
import {  RouterOutlet } from '@angular/router';
import { NavigationSidebar } from '../../shared/components/navigation-sidebar/navigation-sidebar';

@Component({
  selector: 'app-home',
  imports: [Sidebar, NavigationSidebar, RouterOutlet],
  templateUrl: './home.html',
  styleUrl: './home.css',
})
export class Home {
}
