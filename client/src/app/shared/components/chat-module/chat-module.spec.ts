import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ChatModule } from './chat-module';

describe('ChatModule', () => {
  let component: ChatModule;
  let fixture: ComponentFixture<ChatModule>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ChatModule],
    }).compileComponents();

    fixture = TestBed.createComponent(ChatModule);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
