import { Component, inject } from '@angular/core';
import { chatStore } from '../../shared/store/chat.store';
import { environment } from '../../../environments/environment.development';
import { MatGridListModule } from '@angular/material/grid-list';

@Component({
  selector: 'app-image-list',
  imports: [MatGridListModule],
  template: `
    <div class="p-2">
      <mat-grid-list cols="3" rowHeight="1:1" gutterSize="8px">
        @for (image of chatStore.images(); track image.createdAt) {
          <mat-grid-tile>
            <div class="image-wrapper w-100 h-100 d-flex align-items-center justify-content-center border rounded overflow-hidden bg-light">
              <img 
                [src]="imageBaseUrl + image.fileUrl" 
                class="img-fluid chat-msg-img" 
                style="cursor: pointer; object-fit: cover; width: 100%; height: 100%;"
              >
            </div>
          </mat-grid-tile>
        } @empty {
           <div class="text-center p-3 opacity-50">No pictures.</div>
        }
      </mat-grid-list>
    </div>
`
})
export class ImageList {

  readonly imageBaseUrl = environment.imageBaseUrl;
  readonly chatStore = inject(chatStore);
}
