import { TestBed } from '@angular/core/testing';

import { DevAuthService } from './dev-auth-service';

describe('DevAuthService', () => {
  let service: DevAuthService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(DevAuthService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
