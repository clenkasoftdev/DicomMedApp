import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { DicomApiService } from '../../services/dicom-api.services';
import { PatientDetailDto, SeriesDto, InstanceDto } from '../../models/dicom.models';

@Component({
  selector: 'app-patient-detail',
  templateUrl: './patient-detail.component.html',
  styleUrls: ['./patient-detail.component.scss']
})
export class PatientDetailComponent implements OnInit {
  patient: PatientDetailDto | null = null;
  loading = false;
  error: string | null = null;
  
  expandedStudyId: number | null = null;
  expandedSeriesId: number | null = null;
  
  seriesMap: Map<number, SeriesDto[]> = new Map();
  instancesMap: Map<number, InstanceDto[]> = new Map();

  viewingInstanceId: number | null = null;
  showViewer = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private dicomApi: DicomApiService
  ) { }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadPatientDetails(+id);
    }
  }

  loadPatientDetails(patientId: number): void {
    this.loading = true;
    this.error = null;

    this.dicomApi.getPatientDetails(patientId).subscribe({
      next: (patient) => {
        this.patient = patient;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading patient details:', error);
        this.error = 'Failed to load patient details';
        this.loading = false;
      }
    });
  }

  toggleStudy(studyId: number): void {
    if (this.expandedStudyId === studyId) {
      this.expandedStudyId = null;
      this.expandedSeriesId = null;
    } else {
      this.expandedStudyId = studyId;
      this.expandedSeriesId = null;
      this.loadSeriesForStudy(studyId);
    }
  }

  loadSeriesForStudy(studyId: number): void {
    if (this.seriesMap.has(studyId)) {
      return; // Already loaded
    }

    this.dicomApi.getStudySeries(studyId).subscribe({
      next: (series) => {
        this.seriesMap.set(studyId, series);
      },
      error: (error) => {
        console.error('Error loading series:', error);
      }
    });
  }

  toggleSeries(seriesId: number): void {
    if (this.expandedSeriesId === seriesId) {
      this.expandedSeriesId = null;
    } else {
      this.expandedSeriesId = seriesId;
      this.loadInstancesForSeries(seriesId);
    }
  }

  loadInstancesForSeries(seriesId: number): void {
    if (this.instancesMap.has(seriesId)) {
      return; // Already loaded
    }

    this.dicomApi.getSeriesInstances(seriesId).subscribe({
      next: (instances) => {
        this.instancesMap.set(seriesId, instances);
      },
      error: (error) => {
        console.error('Error loading instances:', error);
      }
    });
  }

  downloadInstance(instanceId: number): void {
    this.dicomApi.downloadDicomFile(instanceId).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `instance_${instanceId}.dcm`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);
      },
      error: (error) => {
        console.error('Error downloading file:', error);
      }
    });
  }

  viewInstance(instanceId: number): void {
    this.viewingInstanceId = instanceId;
    this.showViewer = true;
  }

  closeViewer(): void {
    this.showViewer = false;
    this.viewingInstanceId = null;
 }

  formatDate(dateString: string | null): string {
    if (!dateString) return 'N/A';
    const date = new Date(dateString);
    return date.toLocaleDateString();
  }

  formatFileSize(bytes: number): string {
    if (bytes < 1024) return bytes + ' B';
    if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB';
    return (bytes / (1024 * 1024)).toFixed(1) + ' MB';
  }

  goBack(): void {
    this.router.navigate(['/patients']);
  }
}
