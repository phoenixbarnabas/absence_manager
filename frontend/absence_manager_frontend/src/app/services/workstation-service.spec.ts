import { TestBed } from '@angular/core/testing';

import { WorkstationService } from './workstation-service';

describe('WorkstationService', () => {
  let service: WorkstationService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(WorkstationService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
