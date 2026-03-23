import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ChatOptions } from './chat-options';

describe('ChatOptions', () => {
  let component: ChatOptions;
  let fixture: ComponentFixture<ChatOptions>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ChatOptions],
    }).compileComponents();

    fixture = TestBed.createComponent(ChatOptions);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
