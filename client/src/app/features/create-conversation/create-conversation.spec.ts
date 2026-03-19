import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CreateConversation } from './create-conversation';

describe('CreateConversation', () => {
  let component: CreateConversation;
  let fixture: ComponentFixture<CreateConversation>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CreateConversation],
    }).compileComponents();

    fixture = TestBed.createComponent(CreateConversation);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
