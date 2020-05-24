import { TestBed, inject, async } from '@angular/core/testing';

import { HardwareService } from './hardware.service';

describe('HardwareService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({});
  });

  it('should be created',
    inject([HardwareService], (service: HardwareService) => {
      expect(service).toBeTruthy();
    })
  );

  it('should update prop',
      async(
        inject([HardwareService], (service: HardwareService) => {
          service.updateProp('changed');
          service.hardware$.subscribe(hardware => expect(hardware.prop).toBe('changed'));
      })
    )
  );
});
