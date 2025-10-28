import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { DicomApiService } from '../../services/dicom-api.services';
import { PatientDto } from '../../models/dicom.models';

@Component({
  selector: 'app-patient-list',
  templateUrl: './patient-list.component.html',
  styleUrls: ['./patient-list.component.scss']
})
export class PatientListComponent implements OnInit {
  patients: PatientDto[] = [];
  loading = false;
  error: string | null = null;
  displayedColumns: string[] = ['patientId', 'patientName', 'birthDate', 'gender', 'studyCount', 'actions'];

  constructor(
    private dicomApi: DicomApiService,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.loadPatients();
  }

  loadPatients(): void {
    this.loading = true;
    this.error = null;
    
    this.dicomApi.getAllPatients().subscribe({
      next: (patients) => {
        this.patients = patients;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading patients:', error);
        this.error = 'Failed to load patients. Please check if the backend is running.';
        this.loading = false;
      }
    });
  }

  viewPatientDetails(patientId: number): void {
    this.router.navigate(['/patients', patientId]);
  }

  formatDate(dateString: string | null): string {
    if (!dateString) return 'N/A';
    const date = new Date(dateString);
    return date.toLocaleDateString();
  }
}
