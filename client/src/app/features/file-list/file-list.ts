import { Component, inject } from '@angular/core';
import { chatStore } from '../../shared/store/chat.store';
import { getCleanUrl } from '../../core/utils/utils';
import { environment } from '../../../environments/environment.development';

@Component({
  selector: 'app-file-list',
  imports: [],
  template: `@for (file of chatStore.files(); track file.createdAt){
    <a [href]="getCleanUrl(file.fileUrl)" download class="btn btn-sm btn-primary">{{file.fileUrl}}</a>
}`
})
export class FileList {

  readonly chatStore = inject(chatStore);
  readonly imageBaseUrl = environment.imageBaseUrl;

  getCleanUrl(path: string) {
    return getCleanUrl(path, this.imageBaseUrl);
  }
}
