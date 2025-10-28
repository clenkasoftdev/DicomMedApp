import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DicomUploadComponent } from './dicom-upload.component';

describe('DicomUploadComponent', () => {
  let component: DicomUploadComponent;
  let fixture: ComponentFixture<DicomUploadComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ DicomUploadComponent ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(DicomUploadComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
