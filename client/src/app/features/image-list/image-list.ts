import { Component, inject } from '@angular/core';
import { chatStore } from '../../shared/store/chat.store';
import { environment } from '../../../environments/environment.development';

@Component({
  selector: 'app-image-list',
  imports: [],
  template: `@for (image of chatStore.images(); track image.createdAt){
    <div class="message-image-container mb-2">
                  <img [src]="imageBaseUrl + image.fileUrl" class="img-fluid rounded chat-msg-img"
                    style="cursor: pointer; max-height: 300px;">
                </div>
}`
})
export class ImageList {
  
  readonly imageBaseUrl = environment.imageBaseUrl;
  readonly chatStore = inject(chatStore);
}
