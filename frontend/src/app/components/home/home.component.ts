import { Component } from '@angular/core';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent {
  features = [
    {
      icon: 'cloud_upload',
      title: 'Upload DICOM Files',
      description: 'Easily upload single or multiple DICOM files with drag & drop support'
    },
    {
      icon: 'people',
      title: 'Manage Patients',
      description: 'View and organize patient information extracted from DICOM metadata'
    },
    {
      icon: 'folder',
      title: 'Browse Studies',
      description: 'Navigate through patient studies, series, and individual DICOM instances'
    },
    {
      icon: 'download',
      title: 'Download Files',
      description: 'Download DICOM files directly from the browser'
    }
  ];
}
