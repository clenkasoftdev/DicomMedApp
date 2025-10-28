import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { DicomApiService } from '../../services/dicom-api.services';
import { DicomUploadResponseDto } from '../../models/dicom.models';

interface UploadResult extends DicomUploadResponseDto {
  fileName: string;
}

@Component({
  selector: 'app-dicom-upload',
  templateUrl: './dicom-upload.component.html',
  styleUrls: ['./dicom-upload.component.scss']
})
export class DicomUploadComponent {
  selectedFiles: File[] = [];
  uploading = false;
  uploadResults: UploadResult[] = [];
  dragOver = false;

  constructor(
    private dicomApi: DicomApiService,
    private router: Router
  ) { }

  onFileSelected(event: any): void {
    const files: FileList = event.target.files;
    this.addFiles(files);
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.dragOver = true;
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.dragOver = false;
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.dragOver = false;

    const files = event.dataTransfer?.files;
    if (files) {
      this.addFiles(files);
    }
  }

  addFiles(fileList: FileList): void {
    const newFiles = Array.from(fileList).filter(file => 
      file.name.toLowerCase().endsWith('.dcm')
    );
    this.selectedFiles = [...(this.selectedFiles || []), ...(newFiles || [])];
  }

  removeFile(index: number): void {
    this.selectedFiles.splice(index, 1);
  }

  clearAll(): void {
    this.selectedFiles = [];
    this.uploadResults = [];
  }

  async uploadFiles(): Promise<void> {
    if (this.selectedFiles.length === 0) return;

    this.uploading = true;
    this.uploadResults = [];

    // Upload files one by one (could also use batch endpoint)
    for (const file of this.selectedFiles) {
      try {
        const result = await this.dicomApi.uploadDicomFile(file).toPromise();
        if (result) {
          this.uploadResults.push({
            ...result,
            fileName: file.name
          });
        }
      } catch (error: any) {
        console.error('Error uploading file:', file.name, error);
        console.error('Error uploading file:', file.name, error.error.message);
        this.uploadResults.push({
          success: false,
          message: `Failed to upload: ${error.error.message}`,
          fileName: file.name,
          patientId: null,
          studyId: null,
          seriesId: null,
          instanceId: null
        });
      }
    }

    this.uploading = false;
  }

  getSuccessCount(): number {
    return this.uploadResults.filter(r => r.success).length;
  }

  getFailureCount(): number {
    return this.uploadResults.filter(r => !r.success).length;
  }

  viewPatients(): void {
    this.router.navigate(['/patients']);
  }

  formatFileSize(bytes: number): string {
    if (bytes < 1024) return bytes + ' B';
    if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB';
    return (bytes / (1024 * 1024)).toFixed(1) + ' MB';
  }
}
