import { Component, OnInit, Input, ViewChild, ElementRef, AfterViewInit } from '@angular/core';
import * as cornerstone from 'cornerstone-core';
import * as cornerstoneWADOImageLoader from 'cornerstone-wado-image-loader';
import * as dicomParser from 'dicom-parser';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-dicom-viewer',
  templateUrl: './dicom-viewer.component.html',
  styleUrls: ['./dicom-viewer.component.scss']
})
export class DicomViewerComponent implements OnInit, AfterViewInit {
  @ViewChild('dicomImage', { static: false }) dicomImage!: ElementRef;
  @Input() instanceId!: number;

  loading = true;
  error: string | null = null;

  ngOnInit() {
    // Configure cornerstone
    cornerstoneWADOImageLoader.external.cornerstone = cornerstone;
    cornerstoneWADOImageLoader.external.dicomParser = dicomParser;

    cornerstoneWADOImageLoader.configure({
      beforeSend: (xhr: any) => {
        // Add headers if needed for authentication
      }
    });
  }

  ngAfterViewInit() {
    const element = this.dicomImage.nativeElement;
    cornerstone.enable(element);
    this.loadImage();
  }

  /* loadImage() {
  this.loading = true;
  this.error = null;

  const imageUrl = `${environment.apiUrl}/dicom/download/${this.instanceId}`;
  const imageId = `wadouri:${imageUrl}`;
  console.log('Loading DICOM from:', imageUrl); // DEBUG

  cornerstone.loadImage(imageId).then((image: any) => {
    console.log('DICOM Image loaded:', image); // DEBUG
    console.log('Image dimensions:', image.width, 'x', image.height); // DEBUG
    console.log('Window/Level:', image.windowWidth, '/', image.windowCenter); // DEBUG

    const element = this.dicomImage.nativeElement;
    
    // Display the image FIRST
    cornerstone.displayImage(element, image);
    
    // Get viewport
    const viewport = cornerstone.getViewport(element);
    
    // Apply the window/level from the DICOM file
    viewport.voi.windowWidth = image.windowWidth || 892.72;
    viewport.voi.windowCenter = image.windowCenter || 513.55;
    
    // CRITICAL: For MONOCHROME2, don't invert
    viewport.invert = false;
    
    // Apply the corrected viewport
    cornerstone.setViewport(element, viewport);
    
    this.loading = false;
    
    console.log('Image displayed with W/L:', viewport.voi.windowWidth, '/', viewport.voi.windowCenter);

  }).catch((err: any) => {
    console.error('Error loading DICOM image:', err);
    this.error = 'Failed to load DICOM image: ' + err.message;
    this.loading = false;
  });
} */

  loadImage() {
  this.loading = true;
  this.error = null;

  const imageUrl = `${environment.apiUrl}/dicom/download/${this.instanceId}`;
  const imageId = `wadouri:${imageUrl}`;

  cornerstone.loadImage(imageId).then((image: any) => {
    const element = this.dicomImage.nativeElement;
    
    console.log('Image loaded:', image);
    console.log('Dimensions:', image.width, 'x', image.height);
    console.log('Window/Level:', image.windowWidth, '/', image.windowCenter);
    console.log('Min/Max pixels:', image.minPixelValue, '/', image.maxPixelValue);
    
    // Display image
    const viewport = cornerstone.getDefaultViewportForImage(element, image);
    
    // CRITICAL: Set proper window/level
    viewport.voi = {
      windowWidth: image.windowWidth || (image.maxPixelValue - image.minPixelValue),
      windowCenter: image.windowCenter || ((image.maxPixelValue + image.minPixelValue) / 2)
    };
    
    // Display with the calculated viewport
    cornerstone.displayImage(element, image, viewport);

    // DEBUG: Force a render
    setTimeout(() => {
      cornerstone.resize(element, true);
      const vp = cornerstone.getViewport(element);
      console.log('Final viewport after render:', vp);
    }, 500);
        
    this.loading = false;
  }).catch((err: any) => {
    console.error('Error loading DICOM image:', err);
    this.error = 'Failed to load DICOM image: ' + err.message;
    this.loading = false;
  });
}

  resetView() {
    const element = this.dicomImage.nativeElement;
    cornerstone.reset(element);
    this.loadImage();
  }
  
  fitToWindow() {
    const element = this.dicomImage.nativeElement;
    const viewport = cornerstone.getViewport(element);
    
    // Reset zoom to fit
    cornerstone.fitToWindow(element);
    
    this.loadImage(); // Reload with new size
  }
  
}