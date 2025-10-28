import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  PatientDto,
  PatientDetailDto,
  StudyDto,
  SeriesDto,
  InstanceDto,
  DicomUploadResponseDto
} from '../models/dicom.models';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})

export class DicomApiService {
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }

  // Patient endpoints
  getAllPatients(): Observable<PatientDto[]> {
    return this.http.get<PatientDto[]>(`${this.apiUrl}/patients`);
  }

  getPatient(id: number): Observable<PatientDto> {
    return this.http.get<PatientDto>(`${this.apiUrl}/patients/${id}`);
  }

  getPatientDetails(id: number): Observable<PatientDetailDto> {
    return this.http.get<PatientDetailDto>(`${this.apiUrl}/patients/${id}/details`);
  }

  getPatientStudies(id: number): Observable<StudyDto[]> {
    return this.http.get<StudyDto[]>(`${this.apiUrl}/patients/${id}/studies`);
  }

  // Study endpoints
  getStudySeries(studyId: number): Observable<SeriesDto[]> {
    return this.http.get<SeriesDto[]>(`${this.apiUrl}/studies/${studyId}/series`);
  }

  // Series endpoints
  getSeriesInstances(seriesId: number): Observable<InstanceDto[]> {
    return this.http.get<InstanceDto[]>(`${this.apiUrl}/series/${seriesId}/instances`);
  }

  // DICOM file operations
  uploadDicomFile(file: File): Observable<DicomUploadResponseDto> {
    const formData = new FormData();
    formData.append('file', file, file.name);

    return this.http.post<DicomUploadResponseDto>(
      `${this.apiUrl}/dicom/upload`,
      formData
    );
  }

  uploadMultipleDicomFiles(files: File[]): Observable<DicomUploadResponseDto[]> {
    const formData = new FormData();
    files.forEach(file => {
      formData.append('files', file, file.name);
    });

    return this.http.post<DicomUploadResponseDto[]>(
      `${this.apiUrl}/dicom/upload/batch`,
      formData
    );
  }

  downloadDicomFile(instanceId: number): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/dicom/download/${instanceId}`, {
      responseType: 'blob'
    });
  }

  getDicomFileUrl(instanceId: number): string {
    return `${this.apiUrl}/dicom/download/${instanceId}`;
  }
}
