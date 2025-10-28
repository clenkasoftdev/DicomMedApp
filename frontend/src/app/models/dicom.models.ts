// Models matching the backend DTOs

export interface PatientDto {
  id: number;
  patientId: string;
  patientName: string;
  birthDate: string | null;
  gender: string | null;
  studyCount: number;
}

export interface StudyDto {
  id: number;
  studyInstanceUid: string;
  studyDate: string | null;
  studyDescription: string | null;
  accessionNumber: string | null;
  patientName: string;
  seriesCount: number;
}

export interface SeriesDto {
  id: number;
  seriesInstanceUid: string;
  modality: string | null;
  seriesNumber: number | null;
  seriesDescription: string | null;
  instanceCount: number;
}

export interface InstanceDto {
  id: number;
  sopInstanceUid: string;
  instanceNumber: number | null;
  filePath: string;
  fileSize: number;
  rows: number | null;
  columns: number | null;
}

export interface PatientDetailDto {
  id: number;
  patientId: string;
  patientName: string;
  birthDate: string | null;
  gender: string | null;
  studies: StudyDto[];
}

export interface DicomUploadResponseDto {
  success: boolean;
  message: string;
  patientId: number | null;
  studyId: number | null;
  seriesId: number | null;
  instanceId: number | null;
}